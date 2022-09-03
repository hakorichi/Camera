using System;

using Emgu.CV;
using Emgu.Util;
using Emgu.CV.Structure;
using Emgu.CV.GPU;
using Emgu.CV.ML;
using Emgu.CV.Stitching;
using Emgu.CV.OCR;

public class CamFunction
{
    int flipRotatin = 0;
    int Filter = 0;
    int S;
    Boolean V = false;
    Capture c1;
    VideoWriter VideoW;
    Image<Bgr, Byte> im;
    Image<Bgr, Byte> im2;

    public CamFunction()
	{
        c1 = new Capture(0);
        im2 = c1.QueryFrame();
        Application.Idle += GetVideo;
        c1.SetCaptureProperty(Emgu.CV.CvEnum.CAP_PROP.CV_CAP_PROP_FPS, 30);
	}

    private Image<Bgr, Byte> GetVideo()
    {
        im = c1.QueryFrame();
        if (checkBox2.Checked) im = im.Flip(Emgu.CV.CvEnum.FLIP.HORIZONTAL);
        im = im.Mul((double)trackBar1.Value / 100);
        switch (flipRotatin)
        {
            case 1:
                    im = im.Rotate(90, new Bgr());
                    break;
            case 2:
                    im = im.Rotate(180, new Bgr());
                    break;
            case 3:
                    im = im.Rotate(270, new Bgr());
                    break;
        }
        switch (Filter)
        {
            case 1:
                    Image<Gray, byte> im3 = im.Convert<Gray, byte>();
                    Image<Gray, Single> img_final = (im3.Sobel(1, 0, 5));
                    im = img_final.Convert<Bgr, byte>();
                    break;
            case 2:
                    Image<Gray, byte> gray = im.Convert<Gray, byte>();
                    Image<Gray, float> img_final = gray.Sobel(0, 1, 3).Add(gray.Sobel(1, 0, 3)).AbsDiff(new Gray(0.0)).Not();
                    im = img_final.Convert<Bgr, byte>().Not();
                    break;
            case 3:
                    Image<Gray, byte> gray = im.Convert<Gray, byte>();
                    Image<Gray, float> img_final = gray.Sobel(0, 1, 3).Add(gray.Sobel(1, 0, 3)).AbsDiff(new Gray(0.0));
                    im = img_final.Convert<Bgr, byte>().Not();
                    break;
            case 4:
                    im = im.Not();
                    break;
            case 5:
                    Image<Gray, byte> R = im[2].Mul(0.393) + im[1].Mul(0.769) + im[0].Mul(0.189);
                    Image<Gray, byte> G = im[2].Mul(0.349) + im[1].Mul(0.686) + im[0].Mul(0.168);
                    Image<Gray, byte> B = im[2].Mul(0.272) + im[1].Mul(0.534) + im[0].Mul(0.131);
                    im[0] = B; im[1] = G; im[2] = R;
                    break;
        }
        Monitor.Enter(this);
        im2 = im.Convert<Bgr, byte>();
        Monitor.Exit(this);
        return im2;
    }

    public String[] CamList()
    {
            DsDevice[] _SystemCamereas = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);
            String[] s = new String[_SystemCamereas.Length];
            for (int i = 0; i < _SystemCamereas.Length; i++)
            {
                s[i] = _SystemCamereas[i].Name.ToString();
            }
    }

    public void RecordVideo(String Address)
    {
        VideoW = new VideoWriter(Address + "cam" + "_" + DateTime.Now.Year.ToString() + "_" + DateTime.Now.Month.ToString() + "_" + DateTime.Now.Day.ToString() + "_" + DateTime.Now.Hour.ToString() + "_" + DateTime.Now.Minute.ToString() + "_" + DateTime.Now.Second.ToString() + "_" + DateTime.Now.Millisecond.ToString() + ".avi",
                       CvInvoke.CV_FOURCC('M', 'P', '4', '2'),
                       (Convert.ToInt32(30)),
                       im.Width,
                       im.Height,
                       true);

        Thread RV = new Thread(new ParameterizedThreadStart(RecordVideo)); 
        RV.Start();
    }
    private void RecordVideo()
    {
        while (true)
        {
            S = DateTime.Now.Millisecond;
            if (V)
            {
                Monitor.Enter(this);
                VideoW.WriteFrame(im2);
                Monitor.Exit(this);
            }
            S = DateTime.Now.Millisecond - S;
            S = (int)((double)1000 / 30) - S; if (S < 0) S = 0;
            Thread.Sleep(S);
        }
    }

    public void StopVideo()
    {
        Monitor.Enter(this);
        VideoW.Dispose();
        Monitor.Exit(this);
    }

    public void SavePhoto(String Address)
    {
        Image bmp = imageBox1.DisplayedImage.Bitmap;

        // в строку fileName записываем указанный в savedialog полный путь к файлу
        string fileName = Address + "cam" + "_" + DateTime.Now.Year.ToString() + "_" + DateTime.Now.Month.ToString() + "_" + DateTime.Now.Day.ToString() + "_" + DateTime.Now.Hour.ToString() + "_" + DateTime.Now.Minute.ToString() + "_" + DateTime.Now.Second.ToString() + "_" + DateTime.Now.Millisecond.ToString() + ".jpg";
        // Убираем из имени три последних символа (расширение файла)
        string strFilExtn = "jpg";
        // Сохраняем файл в нужном формате и с нужным расширением
        switch (strFilExtn)
        {
            case "bmp":
                bmp.Save(fileName, System.Drawing.Imaging.ImageFormat.Bmp);
                break;
            case "jpg":
                bmp.Save(fileName, System.Drawing.Imaging.ImageFormat.Jpeg);
                break;
            case "gif":
                bmp.Save(fileName, System.Drawing.Imaging.ImageFormat.Gif);
                break;
            case "tif":
                bmp.Save(fileName, System.Drawing.Imaging.ImageFormat.Tiff);
                break;
            case "png":
                bmp.Save(fileName, System.Drawing.Imaging.ImageFormat.Png);
                break;
            default:
                break;
        }
    }

    public void ChendeCam(Int Num,int X, int Y)
    {
        int CamNumber = Num;
        c1.Dispose();
        c1 = new Capture(CamNumber);
        c1.SetCaptureProperty(Emgu.CV.CvEnum.CAP_PROP.CV_CAP_PROP_FPS, 30);
        c1.SetCaptureProperty(Emgu.CV.CvEnum.CAP_PROP.CV_CAP_PROP_FRAME_HEIGHT, X);
        c1.SetCaptureProperty(Emgu.CV.CvEnum.CAP_PROP.CV_CAP_PROP_FRAME_WIDTH,Y);
    }
}
