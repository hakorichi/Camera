using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;
using System.Windows.Forms;

using EDSDKLib;

namespace MIV_cam
{
    public static class Canon
    {

    #region Variables
    static PictureBox LiveViewPicBox;
    static String SavePathText;
    static SDKHandler CameraHandler;
    static List<int> AvList;
    static List<int> TvList;
    static List<int> ISOList;
    static List<Camera> CamList;
    static Bitmap Evf_Bmp;
    static int LVBw, LVBh, w, h;
    static float LVBratio, LVration;
    static int ErrCount;
    static object ErrLock = new object();
    #endregion

    public static void Initialize(Image Bmp)
    {
        try
        {
            LiveViewPicBox = new PictureBox(); LiveViewPicBox.Image = Bmp;
            CameraHandler = new SDKHandler();
            CameraHandler.CameraAdded += new SDKHandler.CameraAddedHandler(SDK_CameraAdded);
            CameraHandler.LiveViewUpdated += new SDKHandler.StreamUpdate(SDK_LiveViewUpdated);
            CameraHandler.ProgressChanged += new SDKHandler.ProgressHandler(SDK_ProgressChanged);
            CameraHandler.CameraHasShutdown += SDK_CameraHasShutdown;
            SavePathText = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "RemotePhoto");
            LVBw = 800;
            LVBh = 600;
            RefreshCamera();
        }
        catch (DllNotFoundException) { ReportError("Canon DLLs not found!", true); }
        catch (Exception ex) { ReportError(ex.Message, true); }
    }

    public static void TakePhoto()
    {
        CameraHandler.TakePhoto();
    }

    private static void Closing(object sender, FormClosingEventArgs e)
    {
        try { if (CameraHandler != null) CameraHandler.Dispose(); }
        catch (Exception ex) { ReportError(ex.Message, false); }
    }

    #region SDK Events

    private static void SDK_ProgressChanged(int Progress)
    {
    }

    private static void SDK_LiveViewUpdated(Stream img)
    {
        try
        {
            Evf_Bmp = new Bitmap(img);
            using (Graphics g = LiveViewPicBox.CreateGraphics())
            {
                LVBratio = LVBw / (float)LVBh;
                LVration = Evf_Bmp.Width / (float)Evf_Bmp.Height;
                if (LVBratio < LVration)
                {
                    w = LVBw;
                    h = (int)(LVBw / LVration);
                }
                else
                {
                    w = (int)(LVBh * LVration);
                    h = LVBh;
                }
                g.DrawImage(Evf_Bmp, 0, 0, w, h);
            }
            Evf_Bmp.Dispose();
        }
        catch (Exception ex) { ReportError(ex.Message, false); }
    }

    private static void SDK_CameraAdded()
    {
        try { RefreshCamera(); }
        catch (Exception ex) { ReportError(ex.Message, false); }
    }

    private static void SDK_CameraHasShutdown(object sender, EventArgs e)
    {
        try { CloseSession(); }
        catch (Exception ex) { ReportError(ex.Message, false); }

    }

    #endregion

    private static void LiveView(object sender, EventArgs e)
    {
        try
        {
            if (!CameraHandler.IsLiveViewOn) { CameraHandler.StartLiveView(); }
            else { CameraHandler.StopLiveView(); }
        }
        catch (Exception ex) { ReportError(ex.Message, false); }
    }

    #region Subroutines

    public static void CloseSession()
    {
        CameraHandler.CloseSession();
    }

    public static List<String> RefreshCamera()
    {
        List<String> s = new List<String>();
        CamList = CameraHandler.GetCameraList();
        foreach (Camera cam in CamList) s.Add(cam.Info.szDeviceDescription);
        return s;
    }

    public static void OpenSession(int I)
    {
        if (I >= 0)
        {
            CameraHandler.OpenSession(CamList[I]);
            string cameraname = CameraHandler.MainCamera.Info.szDeviceDescription;
            if (CameraHandler.GetSetting(EDSDK.PropID_AEMode) != EDSDK.AEMode_Manual) MessageBox.Show("Camera is not in manual mode. Some features might not work!");
            AvList = CameraHandler.GetSettingsList((uint)EDSDK.PropID_Av);
            TvList = CameraHandler.GetSettingsList((uint)EDSDK.PropID_Tv);
            ISOList = CameraHandler.GetSettingsList((uint)EDSDK.PropID_ISOSpeed);
            int wbidx = (int)CameraHandler.GetSetting((uint)EDSDK.PropID_WhiteBalance);
        }
    }

    private static void ReportError(string message, bool lockdown)
    {
        int errc;
        lock (ErrLock) { errc = ++ErrCount; }

        if (lockdown) EnableUI(false);

        if (errc < 4) MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        else if (errc == 4) MessageBox.Show("Many errors happened!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

        lock (ErrLock) { ErrCount--; }
    }
    private static void EnableUI(bool enable)
    {
    }

    #endregion

    public static List<String> GetCamList()
{
    List<String> s = new List<String>();
    CamList = CameraHandler.GetCameraList();
    foreach (Camera cam in CamList) s.Add(cam.Info.szDeviceDescription);
    return s;
}
    public static Bitmap GetBitmap()
    {
        return new Bitmap(LiveViewPicBox.Image);
    }

        public static void Close()
        {
            CloseSession();
            CameraHandler.CloseSession();
            CameraHandler.Dispose();
        }
    }
}
