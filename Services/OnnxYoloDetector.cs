using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using VideoDetectionPOC.DataAccess;
using SessionOptions = Microsoft.ML.OnnxRuntime.SessionOptions;

namespace VideoDetectionPOC.Services
{
    public class OnnxYoloDetector : IDisposable
    {
        private readonly string yoloModelHalf = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "yolomodelhalf") + @"\\yolo11n.onnx";
        private readonly string yoloModelFull = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "yolomodelfull") + @"\\yolo11n.onnx";
        private readonly string framesOutputPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "framesoutput");
        public static string[] cocoLabels = new[]
        {
            "person","bicycle","car","motorbike","aeroplane","bus","train","truck","boat","traffic light",
            "fire hydrant","stop sign","parking meter","bench","bird","cat","dog","horse","sheep","cow",
            "elephant","bear","zebra","giraffe","backpack","umbrella","handbag","tie","suitcase",
            "frisbee","skis","snowboard","sports ball","kite","baseball bat","baseball glove",
            "skateboard","surfboard","tennis racket","bottle","wine glass","cup","fork","knife","spoon",
            "bowl","banana","apple","sandwich","orange","broccoli","carrot","hot dog","pizza","donut",
            "cake","chair","sofa","pottedplant","bed","diningtable","toilet","tvmonitor","laptop",
            "mouse","remote","keyboard","cell phone","microwave","oven","toaster","sink","refrigerator",
            "book","clock","vase","scissors","teddy bear","hair drier","toothbrush"
        };
        private readonly IServiceScopeFactory _scopeFactory;
        private bool _useHalfModel = true; 
        private InferenceSession? session = null;
        public OnnxYoloDetector(bool useGPU, bool useHalfModel, IServiceScopeFactory scopeFactory)
        {
            _useHalfModel = useHalfModel;
            session = useGPU ? CreateGPUSession(0) : CreateCPUSession();
            _scopeFactory = scopeFactory;
        }

        // ------------------------------------------------------------
        // Create GPU session
        // ------------------------------------------------------------
        private InferenceSession CreateGPUSession(int gpuId)
        {
            var so = SessionOptions.MakeSessionOptionWithCudaProvider(gpuId);
            so.GraphOptimizationLevel = GraphOptimizationLevel.ORT_ENABLE_ALL;
            return new InferenceSession(_useHalfModel ? yoloModelHalf : yoloModelFull, so);
        }

        // ------------------------------------------------------------
        // Create CPU session
        // ------------------------------------------------------------
        private InferenceSession CreateCPUSession()
        {
            return new InferenceSession(_useHalfModel ? yoloModelHalf : yoloModelFull);
        }

        public void ProcessFrame(string videoPath, string framePath)
        {
            string frameFileName = Path.GetFileName(framePath);

            // Detect objects
            var detections = DetectObjects(framePath, session);

            var detectionRepository = _scopeFactory.CreateScope().ServiceProvider.GetRequiredService<DetectionRepository>();    
            detectionRepository.AddDetection(videoPath, framePath, detections);

            // Draw bounding boxes and save
            DrawBoundingBoxes(framePath, detections, cocoLabels,
                Path.Combine(framesOutputPath, frameFileName), 640, 640);

            Console.WriteLine($"  Found {detections.Count} objects");
        }

        private List<Detection> DetectObjects(string imagePath, InferenceSession session, float confidenceThreshold = 0.25f, float iouThreshold = 0.45f)
        {
            // Load and preprocess image
            using var image = Image.Load<Rgb24>(imagePath);
            int originalWidth = image.Width;
            int originalHeight = image.Height;

            // YOLO11 expects 640x640 input
            const int inputWidth = 640;
            const int inputHeight = 640;

            // Resize image while maintaining aspect ratio
            image.Mutate(x => x.Resize(new ResizeOptions
            {
                Size = new Size(inputWidth, inputHeight),
                Mode = ResizeMode.Pad
            }));

            if (_useHalfModel)
            {
                // Prepare input tensor - YOLO expects [batch, channels, height, width] format
                var input = new DenseTensor<Float16>(new[] { 1, 3, inputHeight, inputWidth });
                FillTensorWithNormalizedPixels(input, inputHeight, inputWidth, image);
                var inputs = CreateOnnxNamedValueFromTensor(input);
                var results = session.Run(inputs);
                var output = results.First().AsTensor<Float16>();
                var detections = ReadDetectionsFromOutput(output, confidenceThreshold, inputWidth, inputHeight);
                // Apply Non-Maximum Suppression (NMS)
                return ApplyNMS(detections, iouThreshold);
            }
            else
            {
                // Prepare input tensor - YOLO expects [batch, channels, height, width] format
                var input = new DenseTensor<float>(new[] { 1, 3, inputHeight, inputWidth });
                FillTensorWithNormalizedPixels(input, inputHeight, inputWidth, image);
                var inputs = CreateOnnxNamedValueFromTensor(input);
                var results = session.Run(inputs);
                var output = results.First().AsTensor<float>();
                var detections = ReadDetectionsFromOutput(output, confidenceThreshold, inputWidth, inputHeight);
                // Apply Non-Maximum Suppression (NMS)
                return ApplyNMS(detections, iouThreshold);
            }
        }

        private void FillTensorWithNormalizedPixels(DenseTensor<float> input, int height, int width, Image<Rgb24> image)
        {
            // Fill tensor with normalized pixel values (0-1 range)
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var pixel = image[x, y];
                    input[0, 0, y, x] = (pixel.R / 255f);
                    input[0, 1, y, x] = (pixel.G / 255f);
                    input[0, 2, y, x] = (pixel.B / 255f);
                }
            }
        }

        private void FillTensorWithNormalizedPixels(DenseTensor<Float16> input, int height, int width, Image<Rgb24> image)
        {
            // Fill tensor with normalized pixel values (0-1 range)
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var pixel = image[x, y];
                    input[0, 0, y, x] = (Float16)(pixel.R / 255f);
                    input[0, 1, y, x] = (Float16)(pixel.G / 255f);
                    input[0, 2, y, x] = (Float16)(pixel.B / 255f);
                }
            }
        }

        private List<NamedOnnxValue> CreateOnnxNamedValueFromTensor(DenseTensor<float> input)
        {
            return new List<NamedOnnxValue>
            {
                NamedOnnxValue.CreateFromTensor("images", input)
            };
        }

        private List<NamedOnnxValue> CreateOnnxNamedValueFromTensor(DenseTensor<Float16> input)
        {
            return new List<NamedOnnxValue>
            {
                NamedOnnxValue.CreateFromTensor("images", input)
            };
        }

        private List<Detection> ReadDetectionsFromOutput(
            Tensor<float> output,
            float confidenceThreshold,
            int inputWidth,
            int inputHeight)
        {
            var detections = new List<Detection>();
            int numDetections = output.Dimensions[2];
            int numClasses = output.Dimensions[1] - 4;

            for (int i = 0; i < numDetections; i++)
            {
                // Find class with max score
                float maxScore = 0;
                int maxClassId = 0;
                for (int c = 0; c < numClasses; c++)
                {
                    float score = (float)output[0, 4 + c, i];
                    if (score > maxScore)
                    {
                        maxScore = score;
                        maxClassId = c;
                    }
                }

                if (maxScore < confidenceThreshold)
                    continue;

                // Bounding box in center_x, center_y, width, height
                float cx = (float)output[0, 0, i];
                float cy = (float)output[0, 1, i];
                float w = (float)output[0, 2, i];
                float h = (float)output[0, 3, i];

                // Convert to x1, y1, x2, y2
                float x1 = cx - w / 2f;
                float y1 = cy - h / 2f;
                float x2 = cx + w / 2f;
                float y2 = cy + h / 2f;

                detections.Add(new Detection
                {
                    ClassId = maxClassId,
                    Confidence = maxScore,
                    X1 = x1,
                    Y1 = y1,
                    X2 = x2,
                    Y2 = y2
                });
            }

            return detections;
        }

        private List<Detection> ReadDetectionsFromOutput(
            Tensor<Float16> output,
            float confidenceThreshold,
            int inputWidth,
            int inputHeight)
        {
            var detections = new List<Detection>();
            int numDetections = output.Dimensions[2];
            int numClasses = output.Dimensions[1] - 4;

            for (int i = 0; i < numDetections; i++)
            {
                // Find class with max score
                float maxScore = 0;
                int maxClassId = 0;
                for (int c = 0; c < numClasses; c++)
                {
                    float score = (float)output[0, 4 + c, i];
                    if (score > maxScore)
                    {
                        maxScore = score;
                        maxClassId = c;
                    }
                }

                if (maxScore < confidenceThreshold)
                    continue;

                // Bounding box in center_x, center_y, width, height
                float cx = (float)output[0, 0, i];
                float cy = (float)output[0, 1, i];
                float w = (float)output[0, 2, i];
                float h = (float)output[0, 3, i];

                // Convert to x1, y1, x2, y2
                float x1 = cx - w / 2f;
                float y1 = cy - h / 2f;
                float x2 = cx + w / 2f;
                float y2 = cy + h / 2f;

                detections.Add(new Detection
                {
                    ClassId = maxClassId,
                    Confidence = maxScore,
                    X1 = x1,
                    Y1 = y1,
                    X2 = x2,
                    Y2 = y2
                });
            }

            return detections;
        }

        //private List<Detection> ReadDetectionsFromOutput(Tensor<Float16> output, float confidenceThreshold, int originalWidth, int originalHeight, int inputWidth, int inputHeight)
        //{
        //    // Parse YOLO11 output format: [batch, 84, 8400]
        //    // Where 84 = 4 (box coords) + 80 (class scores)
        //    var detections = new List<Detection>();
        //    int numDetections = output.Dimensions[2]; // 8400
        //    int numClasses = output.Dimensions[1] - 4; // 80

        //    for (int i = 0; i < numDetections; i++)
        //    {
        //        // Get class scores
        //        float maxScore = 0;
        //        int maxClassId = 0;

        //        for (int c = 0; c < numClasses; c++)
        //        {
        //            float score = (float)output[0, 4 + c, i];
        //            if (score > maxScore)
        //            {
        //                maxScore = score;
        //                maxClassId = c;
        //            }
        //        }

        //        if (maxScore < confidenceThreshold)
        //            continue;

        //        // Get bounding box (center_x, center_y, width, height format)
        //        float cx = (float)output[0, 0, i];
        //        float cy = (float)output[0, 1, i];
        //        float w = (float)output[0, 2, i];
        //        float h = (float)output[0, 3, i];

        //        // Convert to (x1, y1, x2, y2) format
        //        float x1 = (cx - w / 2);
        //        float y1 = (cy - h / 2);
        //        float x2 = (cx + w / 2);
        //        float y2 = (cy + h / 2);

        //        // Scale back to original image size
        //        float scaleX = (float)originalWidth / inputWidth;
        //        float scaleY = (float)originalHeight / inputHeight;

        //        detections.Add(new Detection
        //        {
        //            ClassId = maxClassId,
        //            Confidence = maxScore,
        //            X1 = (int)(x1 * scaleX),
        //            Y1 = (int)(y1 * scaleY),
        //            X2 = (int)(x2 * scaleX),
        //            Y2 = (int)(y2 * scaleY)
        //        });
        //    }
        //    return detections;
        //}

        private void DrawBoundingBoxes(
            string imagePath,
            List<Detection> detections,
            string[] labels,
            string outputPath,
            int modelInputWidth,
            int modelInputHeight)
        {
            using var image = Image.Load<Rgb24>(imagePath);
            int originalWidth = image.Width;
            int originalHeight = image.Height;

            var colors = new[]
            {
                Color.Red, Color.Blue, Color.Green, Color.Yellow, Color.Cyan,
                Color.Magenta, Color.Orange, Color.Purple, Color.Pink, Color.Lime
            };

            // Compute letterbox scaling
            float rW = (float)modelInputWidth / originalWidth;
            float rH = (float)modelInputHeight / originalHeight;
            float scale = Math.Min(rW, rH);
            float padX = (modelInputWidth - originalWidth * scale) / 2f;
            float padY = (modelInputHeight - originalHeight * scale) / 2f;

            image.Mutate(ctx =>
            {
                foreach (var detection in detections)
                {
                    var color = colors[detection.ClassId % colors.Length];
                    var label = labels[detection.ClassId];

                    // Scale coordinates to original image
                    float x1 = (detection.X1 - padX) / scale;
                    float y1 = (detection.Y1 - padY) / scale;
                    float x2 = (detection.X2 - padX) / scale;
                    float y2 = (detection.Y2 - padY) / scale;

                    // Clamp to image boundaries
                    x1 = Math.Max(0, Math.Min(originalWidth - 1, x1));
                    y1 = Math.Max(0, Math.Min(originalHeight - 1, y1));
                    x2 = Math.Max(0, Math.Min(originalWidth - 1, x2));
                    y2 = Math.Max(0, Math.Min(originalHeight - 1, y2));

                    // Draw bounding box
                    var rect = new RectangleF(x1, y1, x2 - x1, y2 - y1);
                    float thickness = Math.Max(1, originalWidth / 200f);
                    ctx.Draw(color, thickness, rect);

                    // Draw label
                    string text = $"{label} {detection.Confidence:P0}";
                    var font = SystemFonts.CreateFont("Arial", 16, FontStyle.Bold);
                    var textOptions = new TextOptions(font);
                    var textSize = TextMeasurer.MeasureSize(text, textOptions);

                    float labelX = x1;
                    float labelY = Math.Max(0, y1 - textSize.Height - 4);
                    var labelRect = new RectangleF(labelX, labelY, textSize.Width + 8, textSize.Height + 4);

                    ctx.Fill(color, labelRect);
                    ctx.DrawText(text, font, Color.White, new PointF(labelX + 4, labelY + 2));
                }
            });

            image.Save(outputPath);
        }

        private List<Detection> ApplyNMS(List<Detection> detections, float iouThreshold)
        {
            var sortedDetections = detections.OrderByDescending(d => d.Confidence).ToList();
            var result = new List<Detection>();

            while (sortedDetections.Count > 0)
            {
                var best = sortedDetections[0];
                result.Add(best);
                sortedDetections.RemoveAt(0);

                sortedDetections = sortedDetections.Where(d =>
                    d.ClassId != best.ClassId || CalculateIoU(best, d) < iouThreshold
                ).ToList();
            }

            return result;
        }

        private float CalculateIoU(Detection a, Detection b)
        {
            float x1 = Math.Max(a.X1, b.X1);
            float y1 = Math.Max(a.Y1, b.Y1);
            float x2 = Math.Min(a.X2, b.X2);
            float y2 = Math.Min(a.Y2, b.Y2);

            float intersection = Math.Max(0, x2 - x1) * Math.Max(0, y2 - y1);
            float areaA = (a.X2 - a.X1) * (a.Y2 - a.Y1);
            float areaB = (b.X2 - b.X1) * (b.Y2 - b.Y1);
            float union = areaA + areaB - intersection;

            return union > 0 ? (float)intersection / union : 0;
        }

        public void Dispose()
        {
            //throw new NotImplementedException();
        }
    }

    public class Detection
    {
        public int ClassId { get; set; }
        public float Confidence { get; set; }
        public float X1 { get; set; }
        public float Y1 { get; set; }
        public float X2 { get; set; }
        public float Y2 { get; set; }
    }
}
