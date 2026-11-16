import cv2
import tempfile
import shutil
import urllib.request
import os
from fastapi import FastAPI, File, UploadFile
import mediapipe as mp
from mediapipe.tasks import python
from mediapipe.tasks.python import vision

app = FastAPI(title="Golf Vision Pose API")

# Ladda ner Googles tränade pose-modell automatiskt om den saknas
MODEL_PATH = "pose_landmarker_lite.task"
if not os.path.exists(MODEL_PATH):
    url = "https://storage.googleapis.com/mediapipe-models/pose_landmarker/pose_landmarker_lite/float16/1/pose_landmarker_lite.task"
    urllib.request.urlretrieve(url, MODEL_PATH)

# Konfigurera Pose Landmarker
base_options = python.BaseOptions(model_asset_path=MODEL_PATH)
options = vision.PoseLandmarkerOptions(
    base_options=base_options,
    output_segmentation_masks=False
)
detector = vision.PoseLandmarker.create_from_options(options)

@app.get("/")
def read_root():
    return {"status": "Golf Vision API is running"}

@app.post("/analyze-swing")
async def analyze_swing(file: UploadFile = File(...)):
    with tempfile.NamedTemporaryFile(delete=False, suffix=".mp4") as tmp:
        shutil.copyfileobj(file.file, tmp)
        tmp_path = tmp.name

    cap = cv2.VideoCapture(tmp_path)
    fps = cap.get(cv2.CAP_PROP_FPS) or 30.0
    frames_data = []
    frame_idx = 0

    while cap.isOpened():
        success, frame = cap.read()
        if not success:
            break

        # Konvertera BGR till RGB och skapa MediaPipe Image-objekt
        rgb_frame = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)
        mp_image = mp.Image(image_format=mp.ImageFormat.SRGB, data=rgb_frame)

        # Kör detektering
        detection_result = detector.detect(mp_image)

        if detection_result.pose_landmarks and len(detection_result.pose_landmarks) > 0:
            lm = detection_result.pose_landmarks[0]
            
            # Extrahera samma leder med samma indexstruktur
            frames_data.append({
                "frameIndex": frame_idx,
                "timestampSeconds": frame_idx / fps,
                "leftShoulder": {"x": lm[11].x, "y": lm[11].y, "z": lm[11].z},
                "rightShoulder": {"x": lm[12].x, "y": lm[12].y, "z": lm[12].z},
                "leftElbow": {"x": lm[13].x, "y": lm[13].y, "z": lm[13].z},
                "rightElbow": {"x": lm[14].x, "y": lm[14].y, "z": lm[14].z},
                "leftWrist": {"x": lm[15].x, "y": lm[15].y, "z": lm[15].z},
                "rightWrist": {"x": lm[16].x, "y": lm[16].y, "z": lm[16].z},
                "leftHip": {"x": lm[23].x, "y": lm[23].y, "z": lm[23].z},
                "rightHip": {"x": lm[24].x, "y": lm[24].y, "z": lm[24].z},
                "leftKnee": {"x": lm[25].x, "y": lm[25].y, "z": lm[25].z},
                "rightKnee": {"x": lm[26].x, "y": lm[26].y, "z": lm[26].z}
            })
        frame_idx += 1

    cap.release()
    try:
        os.remove(tmp_path)
    except Exception:
        pass

    return frames_data