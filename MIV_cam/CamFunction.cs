using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

using Emgu.CV;
using Emgu.Util;
using Emgu.CV.Structure;
using Emgu.CV.GPU;
using Emgu.CV.ML;
using Emgu.CV.OCR;

using DirectShowLib;

public static class CamFunction
{
    #region Variables
    private static Mutex mut = new Mutex();
    public static int flipRotatin = 0;
    public static int Filter = 0;
    static int S;
    public static bool Mirror_double = false;
    public static bool Mirror = false;
    static Boolean V = false;
    static Thread RV;
    static Capture c1;
    static VideoWriter VideoW;
    static Image<Bgr, Byte> im;
    static Image<Bgr, Byte> im2;
    public static Double brightness = 100;
    public static Double contrast = 100;
    public static Double levelB = 100;
    public static Double levelG = 100;
    public static Double levelR = 100;
    #endregion

    public static void Initialize()
	{
        c1 = new Capture();
        im2 = c1.QueryFrame();
        
        c1.SetCaptureProperty(Emgu.CV.CvEnum.CAP_PROP.CV_CAP_PROP_FPS, 30);
	}

    public static Bitmap GetBitmap()
    {
        im = c1.QueryFrame();
        //im = new Image<Bgr, Byte>("D:/Filter1.jpg");
        return GetImage(im).Bitmap;
    }
    public static Bitmap FilterBitmap(Bitmap Bmp)
    {
        Image<Bgr, Byte> IMUP = new Image<Bgr, Byte>(Bmp);
        return GetImage(IMUP).Bitmap;
    }
    private static Image<Bgr, Byte> GetImage(Image<Bgr, Byte> IMUP)
    {
        if (Mirror) IMUP = IMUP.Flip(Emgu.CV.CvEnum.FLIP.HORIZONTAL);

        if (Mirror_double) IMUP = Mirror_double_function(IMUP);

            if (brightness != 100)
        {
            if (brightness > 100)
                IMUP += (byte)(((brightness - 100) / 100) * 255); 
            else
                IMUP -= (byte)(((100 - brightness) / 100) * 255); 
        }
        if (contrast != 100)
        {
            IMUP *= (contrast * contrast / 10000); 
        }
        if (levelB != 100) IMUP[0] = IMUP[0].Mul(levelB / 100);
        if (levelG != 100) IMUP[1] = IMUP[1].Mul(levelG / 100);
        if (levelR != 100) IMUP[2] = IMUP[2].Mul(levelR / 100);

        switch (flipRotatin)
        {
            case 1:
                IMUP = IMUP.Rotate(90, new Bgr());
                    break;
            case 2:
                    IMUP = IMUP.Rotate(180, new Bgr());
                    break;
            case 3:
                    IMUP = IMUP.Rotate(270, new Bgr());
                    break;
        }
        switch (Filter)
        {
            case 1: IMUP = Filter1(IMUP); break;
            case 2: IMUP = Filter2(IMUP); break;
            case 3: IMUP = Filter3(IMUP); break;
            case 4: IMUP = Filter4(IMUP); break;
            case 5: IMUP = Filter5(IMUP); break;
            case 6: IMUP = Filter6(IMUP); break;
            case 7: IMUP = Filter7(IMUP); break;
            case 8: IMUP = Filter8(IMUP); break;
            case 9: IMUP = Filter9(IMUP); break;
        }

        mut.WaitOne();
        im2 = IMUP;
        mut.ReleaseMutex();
        return IMUP;
    }

    public static List<String> CamList()
    {
            DsDevice[] _SystemCamereas = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);
             List<String> s = new  List<String>();
            for (int i = 0; i < _SystemCamereas.Length; i++)
            {
                s.Add(_SystemCamereas[i].Name.ToString());
            }
            return s;
    }

    public static void RecordVideo(String Address)
    {
        V = true;
        VideoW = new VideoWriter(Address + "cam" + "_" + DateTime.Now.Year.ToString() + "_" + DateTime.Now.Month.ToString() + "_" + DateTime.Now.Day.ToString() + "_" + DateTime.Now.Hour.ToString() + "_" + DateTime.Now.Minute.ToString() + "_" + DateTime.Now.Second.ToString() + "_" + DateTime.Now.Millisecond.ToString() + ".avi",
                       CvInvoke.CV_FOURCC('M', 'P', '4', '2'),
                       (Convert.ToInt32(30)),
                       im.Width,
                       im.Height,
                       true);

        RV = new Thread(RecordVideoPrivate); 
        RV.Start();
    }
    private static void RecordVideoPrivate()
    {
        while (V)
        {
            S = DateTime.Now.Millisecond;
            mut.WaitOne();
            VideoW.WriteFrame(im2);
            mut.ReleaseMutex();
            S = DateTime.Now.Millisecond - S;
            S = 33 - S; if (S < 0) S = 0;
            Thread.Sleep(S);
        }
    }
    public static void StopVideo()
    {
        V = false;
        if  (VideoW!=null)VideoW.Dispose();
    }
    public static Boolean EnabledVideo()
    {
        return V;
    }

    public static void SavePhoto(String Address)
    {

        mut.WaitOne();
        Image bmp = im2.Bitmap;
        mut.ReleaseMutex();

        string fileName = Address + "cam" + "_" + DateTime.Now.Year.ToString() + "_" + DateTime.Now.Month.ToString() + "_" + DateTime.Now.Day.ToString() + "_" + DateTime.Now.Hour.ToString() + "_" + DateTime.Now.Minute.ToString() + "_" + DateTime.Now.Second.ToString() + "_" + DateTime.Now.Millisecond.ToString() + ".jpg";
        bmp.Save(fileName, System.Drawing.Imaging.ImageFormat.Jpeg);
    }

    public static void ChendeCam(int Num,int X, int Y)
    {
        c1.Dispose();
        c1 = new Capture(Num);
        c1.SetCaptureProperty(Emgu.CV.CvEnum.CAP_PROP.CV_CAP_PROP_FPS, 30);
    }
    public static void Reset()
    {
        brightness = 100;
        contrast = 100;
        levelB = 100;
        levelG = 100;
        levelR = 100;
    }

    public static void close()
    {
        CamFunction.StopVideo();
        c1.Dispose();
    }

    #region Filtres
    private static Image<Bgr, Byte> Filter1(Image<Bgr, Byte> ImputImage)
    {
        Image<Gray, byte> im3 = ImputImage.Convert<Gray, byte>();
        Image<Gray, Single> img_final = (im3.Sobel(1, 0, 5));
        return img_final.Convert<Bgr, byte>();
    }
    private static Image<Bgr, Byte> Filter2(Image<Bgr, Byte> ImputImage)
    {
        Image<Gray, byte> gray = ImputImage.Convert<Gray, byte>();
        Image<Gray, float> img_final = gray.Sobel(0, 1, 3).Add(gray.Sobel(1, 0, 3)).AbsDiff(new Gray(0.0));
        ImputImage = ImputImage.Min(ImputImage.Or(gray.Convert<Bgr, byte>()));
        return  ImputImage.Not().Add(img_final.Convert<Bgr, byte>().Mul(3)).Not();
    }
    private static Image<Bgr, Byte> Filter3(Image<Bgr, Byte> ImputImage)
    {
        Image<Gray, byte> gray = ImputImage.Convert<Gray, byte>();
        Image<Gray, float> img_final = gray.Sobel(0, 1, 3).Add(gray.Sobel(1, 0, 3)).AbsDiff(new Gray(0.0));
        return img_final.Convert<Bgr, byte>().Mul(2).Min(200).Not();
    }
    private static Image<Bgr, Byte> Filter4(Image<Bgr, Byte> ImputImage)
    {
        return ImputImage.Not();
    }
    private static Image<Bgr, Byte> Filter5(Image<Bgr, Byte> ImputImage)
    {
        Image<Gray, byte> R = ImputImage[2].Mul(0.393) + ImputImage[1].Mul(0.769) + ImputImage[0].Mul(0.189);
        Image<Gray, byte> G = ImputImage[2].Mul(0.349) + ImputImage[1].Mul(0.686) + ImputImage[0].Mul(0.168);
        Image<Gray, byte> B = ImputImage[2].Mul(0.272) + ImputImage[1].Mul(0.534) + ImputImage[0].Mul(0.131);
        ImputImage[0] = B; ImputImage[1] = G; ImputImage[2] = R;
        return ImputImage;
    }
    private static Image<Bgr, Byte> Filter6(Image<Bgr, Byte> ImputImage)
    {
        return ImputImage.Convert<Gray, byte>().Convert<Bgr, byte>();
    }
    private static Image<Bgr, Byte> Filter7(Image<Bgr, Byte> ImputImage)
    {
        Image<Gray, byte> gray1 = ImputImage[0];
        ImputImage[0] = ImputImage[0].Mul(0);
        Image<Gray, byte> gray = ImputImage.Convert<Gray, byte>();
        ImputImage = gray.Convert<Bgr, byte>();
        ImputImage[0] = ImputImage[0].Max(gray1);
        return ImputImage;
    }
    private static Image<Bgr, Byte> Filter8(Image<Bgr, Byte> ImputImage)
    {
        Image<Gray, byte> gray1 = ImputImage[1];
        ImputImage[1] = ImputImage[1].Mul(0);
        Image<Gray, byte> gray = ImputImage.Convert<Gray, byte>();
        ImputImage = gray.Convert<Bgr, byte>();
        ImputImage[1] = ImputImage[1].Max(gray1);
        return ImputImage;
    }
    private static Image<Bgr, Byte> Filter9(Image<Bgr, Byte> ImputImage)
    {
        Image<Gray, byte> gray1 = ImputImage[2];
        ImputImage[2] = ImputImage[2].Mul(0);
        Image<Gray, byte> gray = ImputImage.Convert<Gray, byte>();
        ImputImage = gray.Convert<Bgr, byte>();
        ImputImage[2] = ImputImage[2].Max(gray1);
        return ImputImage;
    }

    private static Image<Bgr, Byte> Mirror_double_function(Image<Bgr, Byte> ImputImage)
    {
        Rectangle roi_left = new Rectangle(0, 0, (int)(ImputImage.Width / 2), ImputImage.Height);
        Rectangle roi_Right = new Rectangle(ImputImage.Width / 2, 0, (int)(ImputImage.Width / 2), ImputImage.Height);

        Image<Bgr, Byte> ImputImage_left = new Image<Bgr, byte>(ImputImage.Width / 2, ImputImage.Height);
        Image<Bgr, Byte> ImputImage_Right = new Image<Bgr, byte>(ImputImage.Width / 2, ImputImage.Height);


        ImputImage_left = ImputImage.Copy(roi_left);
        ImputImage_Right = ImputImage_left.Flip(Emgu.CV.CvEnum.FLIP.HORIZONTAL);



        ImputImage.ROI = roi_left;
        ImputImage_left.CopyTo(ImputImage);
        //CvInvoke.cvCopy(ImputImage, ImputImage_left, IntPtr.Zero);
        ImputImage.ROI = roi_Right;
        ImputImage_Right.CopyTo(ImputImage);
        //CvInvoke.cvCopy(ImputImage, ImputImage_left, IntPtr.Zero);
        ImputImage.ROI = Rectangle.Empty;
        return ImputImage;
    }
    #endregion
}
