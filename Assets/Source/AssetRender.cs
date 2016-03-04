using UnityEngine;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

[RequireComponent(typeof(Camera))]
public class AssetRender : MonoBehaviour
{
    private HttpListener _HttpListener = null;
    private Thread _Thread = null;
    private ManualResetEvent _ImageReadyEvent = new ManualResetEvent(false);
    private bool _TakeSnapShot = false;
    private byte[] _Image = null;

    //---------------------------------------------------------------------------------------------
    public int width = 1024;
    public int height = 1024;
    public int port = 4000;

    //---------------------------------------------------------------------------------------------
    public void Start()
    {
        // Grab the application parameters.
        ParseCommandLineArguments();

        // Start the webserver.
        _HttpListener = new HttpListener();
        _HttpListener.Prefixes.Add(String.Format("http://*:{0}/", port));
        _HttpListener.Start();

        // Handle web requests in a separate thread.
        _Thread = new Thread(Worker);
        _Thread.IsBackground = true;
        _Thread.Start();

        // Setup a render texture which is used for taking snapshots of the rendered product.
        RenderTexture theRenderTexture = new RenderTexture(width, height, 16, RenderTextureFormat.ARGB32);
#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX || UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        // On OSX and Windows setting the antiAliasing to anything higher then 1 results in fully grey images.
        // See https://issuetracker.unity3d.com/issues/texture2d-dot-readpixels-fails-if-rendertexture-has-anti-aliasing-set
        theRenderTexture.antiAliasing = 1;
#else
        theRenderTexture.antiAliasing = 2;
#endif
        theRenderTexture.Create();

        // Assign the render texture to the camera.
        GetComponent<Camera>().targetTexture = theRenderTexture;
        RenderTexture.active = theRenderTexture;
    }

    public void Update()
    {
#if UNITY_STANDALONE
        // Render the camera ourself while running headless/batchmode.
        if (_TakeSnapShot && SystemInfo.graphicsDeviceID == 0)
            GetComponent<Camera>().Render();
#endif
    }

    public void OnPostRender()
    {
        if (!_TakeSnapShot)
            return;
        _TakeSnapShot = false;

        RenderTexture theRenderTexture = GetComponent<Camera>().targetTexture as RenderTexture;
        RenderTexture.active = theRenderTexture;

        Texture2D theTexture = new Texture2D(width, height, TextureFormat.ARGB32, false);
        theTexture.ReadPixels(new Rect(0, 0, width, height), 0, 0, false);
        theTexture.Apply(false);
        _Image = theTexture.EncodeToPNG();
        GameObject.Destroy(theTexture);

        // Notify other thread.
        _ImageReadyEvent.Set();
    }

    public void OnApplicationQuit()
    {
        _HttpListener.Stop();
        if (_Thread.IsAlive)
            _Thread.Abort();
    }

    private void Worker()
    {
        while (_HttpListener.IsListening)
        {
            try
            {
                OnListenerConnected(_HttpListener.GetContext());
            }
            catch (HttpListenerException aException)
            {
                Debug.LogWarning(aException);
            }
            catch (SocketException aException)
            {
                Debug.LogWarning(aException);
            }
            catch (Exception aException)
            {
                Debug.LogException(aException);
            }
            Thread.Sleep(10);
        }
    }

    public void OnListenerConnected(HttpListenerContext aContext)
    {
        try {
            HttpListenerRequest theRequest = aContext.Request;
            System.Console.WriteLine("Recieved request: \"{0}\".", theRequest.Url);
            _TakeSnapShot = true;
            _ImageReadyEvent.WaitOne();
            HttpListenerResponse theResponse = aContext.Response;
            if (_Image != null && _Image.Length > 0)
            {
                theResponse.ContentType = "image/png";
                theResponse.ContentLength64 = _Image.Length;
                theResponse.OutputStream.Write(_Image, 0, _Image.Length);
                theResponse.OutputStream.Close();
            }
            else
            {
                theResponse.StatusCode = (int)HttpStatusCode.InternalServerError;
                using (StreamWriter theStreamWriter = new StreamWriter(theResponse.OutputStream))
                {
                    theStreamWriter.WriteLine("Unknown server error");
                }
            }
            theResponse.Close();
        } finally {
            _ImageReadyEvent.Reset();
        }
    }

    protected void ParseCommandLineArguments()
    {
        string[] theArguments = Environment.GetCommandLineArgs();
        for (int theIndex = 0; theIndex < theArguments.Length; theIndex++)
        {
            Debug.Log(String.Format("{0}: {1}", theIndex, theArguments[theIndex]));
        }
    }
}