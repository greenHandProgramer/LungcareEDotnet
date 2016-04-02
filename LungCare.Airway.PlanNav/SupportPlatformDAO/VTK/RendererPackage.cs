/*=========================================================================

  Program:   集翔多维导航服务
  Language:  C#

  Copyright (c) 北京集翔多维信息技术有限公司. All rights reserved.

=========================================================================*/
using System;
using Kitware.VTK;
using System.Threading;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
//using SharpGL;
//using SharpGL.Enumerations;

namespace LungCare.SupportPlatform.SupportPlatformDAO.VTK
{
    public partial class RendererPackage
    {
        internal void MoveRight()
        {
            double[] viewUp = Camera.GetViewUp();
            double[] directionOfProjection = Camera.GetDirectionOfProjection();
            double[] viewLeft = VTKUtil.Cross(viewUp, directionOfProjection);

            double[] cameraPosition = Camera.GetPosition();
            double[] focalPosition = Camera.GetFocalPoint();

            double[] newCameraPosition = VTKUtil.Extend2AlongDirection(cameraPosition, viewLeft, 1);
            double[] newFocalPosition = VTKUtil.Extend2AlongDirection(focalPosition, viewLeft, 1);

            Camera.SetPosition(newCameraPosition);
            Camera.SetFocalPoint(newFocalPosition);
        }

        internal void MoveLeft()
        {
            double[] viewUp = Camera.GetViewUp();
            double[] directionOfProjection = Camera.GetDirectionOfProjection();
            double[] viewLeft = VTKUtil.Cross(viewUp, directionOfProjection);

            double[] cameraPosition = Camera.GetPosition();
            double[] focalPosition = Camera.GetFocalPoint();

            double[] viewRight = new double[]{
                -viewLeft[0],
                -viewLeft[1],
                -viewLeft[2]
            };

            double[] newCameraPosition = VTKUtil.Extend2AlongDirection(cameraPosition, viewRight, 1);
            double[] newFocalPosition = VTKUtil.Extend2AlongDirection(focalPosition, viewRight, 1);

            Camera.SetPosition(newCameraPosition);
            Camera.SetFocalPoint(newFocalPosition);
        }

        internal void MoveDown()
        {
            double[] viewUp = Camera.GetViewUp();

            double[] cameraPosition = Camera.GetPosition();
            double[] focalPosition = Camera.GetFocalPoint();

            double[] newCameraPosition = VTKUtil.Extend2AlongDirection(cameraPosition, viewUp, 1);
            double[] newFocalPosition = VTKUtil.Extend2AlongDirection(focalPosition, viewUp, 1);
            //newCameraPosition = cameraPosition.Add(viewUp);
            //newFocalPosition = focalPosition.Add(viewUp);

            Console.WriteLine(Camera);
            Camera.SetPosition(newCameraPosition);
            Camera.SetFocalPoint(newFocalPosition);
            Console.WriteLine(Camera);
        }

        internal void MoveUp()
        {
            double[] viewUp = Camera.GetViewUp();
            viewUp[0] *= -1;
            viewUp[1] *= -1;
            viewUp[2] *= -1;

            double[] cameraPosition = Camera.GetPosition();
            double[] focalPosition = Camera.GetFocalPoint();

            double[] newCameraPosition = VTKUtil.Extend2AlongDirection(cameraPosition, viewUp, 1);
            double[] newFocalPosition = VTKUtil.Extend2AlongDirection(focalPosition, viewUp, 1);

            Camera.SetPosition(newCameraPosition);
            Camera.SetFocalPoint(newFocalPosition);
        }

        internal void FireModifiedEvent()
        {
            RenderWindowInteractor.InvokeEvent((int)Kitware.VTK.vtkCommand.EventIds.ModifiedEvent);
        }

        internal vtkRenderer AddTopmostRenderer(vtkRenderWindow renwin, vtkRenderer mainRenderer)
        {
            int oldNumberOfRenderer = renwin.GetNumberOfLayers();
            //Console.WriteLine(string.Format("oldNumberOfRenderer = {0}", oldNumberOfRenderer));
            int newNumberOfRenderer = oldNumberOfRenderer + 1;

            mainRenderer.SetLayer(0);

            vtkRenderer topRenderer = vtkRenderer.New();
            topRenderer.SetViewport(0, 0, 1, 1);
            topRenderer.SetLayer(newNumberOfRenderer - 1);
            topRenderer.InteractiveOff();

            renwin.SetNumberOfLayers(newNumberOfRenderer);
            renwin.AddRenderer(topRenderer);

            topRenderer.SetActiveCamera(mainRenderer.GetActiveCamera());

            return topRenderer;
        }

        public void SetWatermarkOpacity(double opacity)
        {
            if (_watermarkPackage != null)
            {
                _watermarkPackage.SetOpacity(opacity);
            }
        }

        //internal void PlaceWatermark(string filename)
        //{
        //    _watermarkPackage = new WatermarkPackage(_renwin, _renderer, VTKUtil.ReadPNG(filename));
        //}

        public void LogoOn(string pngFileName)
        {
            vtkPNGReader reader = vtkPNGReader.New();
            reader.SetFileName(pngFileName);
            reader.Update();
            //Console.WriteLine(reader.GetOutput());

            vtkLogoRepresentation logoRepresentation = vtkLogoRepresentation.New();
            logoRepresentation.SetImage(reader.GetOutput());
            logoRepresentation.SetPosition(0, 0);
            logoRepresentation.SetPosition2(1, 1);
            logoRepresentation.GetImageProperty().SetOpacity(.3);

            //vtkLogoWidget logoWidget = vtkLogoWidget.New();
            ////logoWidget.SetInteractor(this.RenderWindowInteractor);
            //logoWidget.SetRepresentation(logoRepresentation);
            ////logoWidget.On();
            ////logoWidget.SetCurrentRenderer(renderer);
            //logoWidget.SetDefaultRenderer(renderer);

            reader.Dispose();
        }

        public SpherePackage cursor;

        //internal XmlPolyDataPackage SetCurvePackage(string name, string file)
        //{
        //    if (!polyDataDict.ContainsKey(file))
        //    {
        //        polyDataDict[name] = CreatePolyDataPackage(file);
        //    }
        //    polyDataDict[name].PolyData = AirwayCT浏览界面.VTKPackageLink.CurveUtil.CurveEntity2PolyData(file);
        //    return polyDataDict[name];
        //}

        public void SetCursorPosition(double[] pos)
        {
            cursor.VisibilityOn();
            cursor.SetPosition(pos);
        }

        //public double[] CursorPosition
        //{
        //    get { return cursor.GetPosition(); }
        //}

        internal XmlPolyDataPackage CreatePolyDataPackage(string file)
        {
            return new XmlPolyDataPackage(file, _renderer);
        }

        internal XmlPolyDataPackage SetPolyDataPackage(string file)
        {
            return SetPolyDataPackage(Guid.NewGuid().ToString("N"), file);
        }

        internal XmlPolyDataPackage SetPolyDataPackage(string name, string file)
        {
            if (!polyDataDict.ContainsKey(name))
            {
                polyDataDict[name] = CreatePolyDataPackage(file);
            }
            polyDataDict[name].PolyData = VTKUtil.ReadPolyData(file);
            return polyDataDict[name];
        }

        internal static RendererPackage CreateRendererPackage(Control control)
        {
            return new RendererPackage(control);
        }

        static RendererPackage()
        {
            if (Process.GetCurrentProcess().ProcessName != "CAOSim")
            {
                //Process.GetCurrentProcess().Kill();
            }
            //if (!Directory.Exists(@"C:\CAOSim"))
            //{
            //    Process.GetCurrentProcess().Kill();
            //}
            //if (!Directory.Exists(@"C:\CAOSim\cfg\GammaNailing"))
            //{
            //    Process.GetCurrentProcess().Kill();
            //}
        }

        internal IList<vtkActor> AllActors
        {
            get
            {
                List<vtkActor> ret = new List<vtkActor>();

                for (int i = 0; i < _renderer.GetActors().GetNumberOfItems(); ++i)
                {
                    ret.Add(vtkActor.SafeDownCast(_renderer.GetActors().GetItemAsObject(i)));
                }
                return ret;
            }
        }

        internal vtkRenderWindow RenderWindow
        {
            get
            {
                return _renwin;
            }
        }

        internal vtkRenderWindowInteractor RenderWindowInteractor
        {
            get
            {
                return _iren;
            }
        }

        internal void SetCamera(double[] pos, double[] focal)
        {
            vtkCamera activeCamera = _renderer.GetActiveCamera();

            activeCamera.SetPosition(pos[0], pos[1], pos[2]);
            activeCamera.SetFocalPoint(focal[0], focal[1], focal[2]);

            double[] cross1 = VTKUtil.Cross(VTKUtil.Normalize(pos, focal), new double[] { 0, 1, 0 });
            double[] cross2 = VTKUtil.Cross(VTKUtil.Normalize(pos, focal), cross1);

            activeCamera.SetViewUp(cross2[0], cross2[1], cross2[2]);
        }

        internal void SetCamera(double[] pos, double[] focal, double[] viewUp)
        {
            vtkCamera activeCamera = _renderer.GetActiveCamera();

            activeCamera.SetPosition(pos[0], pos[1], pos[2]);
            activeCamera.SetFocalPoint(focal[0], focal[1], focal[2]);
            activeCamera.SetViewUp(viewUp[0], viewUp[1], viewUp[2]);

            activeCamera.OrthogonalizeViewUp();
        }

        internal void GradientOn()
        {
            Renderer.GradientBackgroundOn();
            Renderer.SetBackground(0, 0, 0);
            Renderer.SetBackground2(0, 0, 1);
            //Renderer.SetBackground2(0.18, 0.57, 0.82);
            Renderer.SetGradientBackground(true);
        }

        internal void SetCameraViewAngleAndLightConeAngle(double angle)
        {
            Camera.SetViewAngle(angle);
            Light.SetConeAngle(angle);
        }

        internal vtkRenderer Renderer
        {
            get
            {
                return _renderer;
            }
        }

        internal vtkCamera Camera
        {
            get
            {
                return _renderer.GetActiveCamera();
            }
        }

        internal vtkVolumePicker WorldPointPicker
        {
            get { return _vtkWorldPointPicker; }
        }

        private readonly vtkVolumePicker _vtkWorldPointPicker = vtkVolumePicker.New();
        private readonly vtkPicker _vtkPicker = vtkPicker.New();

        readonly vtkRenderWindow _renwin;
        readonly vtkRenderWindowInteractor _iren;
        readonly vtkRenderer _renderer;

        internal void PickActor()
        {
            _iren.SetPicker(_vtkPicker);

            _iren.RemoveObserver((int)vtkCommand.EventIds.LeftButtonPressEvent);

            // TODO FIXME
            //    iren.AddObserver((int)Kitware.VTK.vtkCommand.EventIds.LeftButtonPressEvent, new vtkDotNetCallback(ActorPickedMethod));
        }

        internal void PickPoint()
        {
            _iren.SetPicker(_vtkWorldPointPicker);

            _iren.RemoveObserver((int)vtkCommand.EventIds.LeftButtonPressEvent);

            // TODO FIXME
            //iren.AddObserver((int)Kitware.VTK.vtkCommand.EventIds.LeftButtonPressEvent, new vtkDotNetCallback(MarchingCubeMousePress));
        }

        internal void AttachActorCameraSwitch(System.Windows.Forms.Button actor, System.Windows.Forms.Button camera)
        {
            actor.Click += delegate { this.InteractorStyleTrackballActor(); };
            camera.Click += delegate { this.InteractorStyleTrackballCamera(); };
        }

        public void 冠状位()
        {
            正面观();
        }

        public void 矢状位()
        {
            vtkCamera camera = _renderer.GetActiveCamera();

            camera.SetFocalPoint(0, 0, 0);
            camera.SetPosition(100, 0, 0);
            camera.SetViewUp(0, 0, -1);
        }

        public void 轴位()
        {
            vtkCamera camera = _renderer.GetActiveCamera();

            camera.SetFocalPoint(0, 0, 0);
            camera.SetPosition(0, 0, 1000);
            camera.SetViewUp(0, 1, 0);
        }

        public void 正面观()
        {
            vtkCamera camera = _renderer.GetActiveCamera();
            double[] bound = new double[] { 0, 0, 0, 0, 0, 0 };

            camera.SetFocalPoint(
                (bound[0] + bound[1]) / 2,
                (bound[2] + bound[3]) / 2,
                (bound[4] + bound[5]) / 2);

            camera.SetPosition(
                (bound[0] + bound[1]) / 2,
                1000,
                (bound[4] + bound[5]) / 2);

            camera.SetViewUp(0, 0, -1);
        }

        internal RendererPackage(Control control)
        {
            _renwin = vtkRenderWindow.New();
            _renwin.SetParentId(control.Handle);
            _renwin.SetSize(control.Width, control.Height);

            _renderer = vtkRenderer.New();
            _renderer.SetLayer(0);

            _renwin.AddRenderer(_renderer);
            _iren = vtkRenderWindowInteractor.New();
            _iren.SetRenderWindow(_renwin);

            vtkInteractorStyleTrackballCamera style = vtkInteractorStyleTrackballCamera.New();
            _iren.SetInteractorStyle(style);
            //iren.SetInteractorStyle(new vtkInteractorStyleFlight());

            // TODO FIX ME
            _iren.LeftButtonPressEvt += new vtkObject.vtkObjectEventHandler(iren_LeftButtonPressEvt);
            // iren.AddObserver((int)Kitware.VTK.vtkCommand.EventIds.LeftButtonPressEvent, new vtkDotNetCallback(MarchingCubeMousePress));
            // iren.AddObserver((int)Kitware.VTK.vtkCommand.EventIds.RenderEvent, new vtkDotNetCallback(RenderEvent));

            _iren.SetPicker(_vtkWorldPointPicker);

            // DepthPeeling
            //renwin.SetMultiSamples(0);
            //renwin.SetAlphaBitPlanes(1);
            //renwin.AlphaBitPlanesOn();

            //renwin.SetDesiredUpdateRate(20);

            //renderer.SetUseDepthPeeling(1);
            //renderer.SetMaximumNumberOfPeels(100);
            //renderer.SetOcclusionRatio(0.5);
            control.Resize += new EventHandler(control_Resize);
            this.control = control;

            //cursor = new SpherePackage(_renderer);
            //cursor.SetRadius(1);
            //cursor.SetResolution(20, 20);
            //cursor.VisibilityOff();

            //Corner = new CornerAnnotationActorPackage(_renderer);
        }

        internal CornerAnnotationActorPackage Corner;
        public void SetTitle(string title)
        {
            Corner.SetLeftTop(title);
        }

        public void SetLeftDown(string title)
        {
            Corner.SetLeftDownText(title);
        }

        public void SetRightDown(string title)
        {
            Corner.SetRightDownText(title);
        }

        internal void OpenGLOn()
        {
            // Might be buggy for ImageWidget
            _renderer.StartEvt += new vtkObject.vtkObjectEventHandler(renderer_StartEvt);
            _renderer.EndEvt += new vtkObject.vtkObjectEventHandler(renderer_EndEvt);
        }

        internal void OpenGLOff()
        {
            _renderer.StartEvt -= new vtkObject.vtkObjectEventHandler(renderer_StartEvt);
            _renderer.EndEvt -= new vtkObject.vtkObjectEventHandler(renderer_EndEvt);
        }

        void renderer_EndEvt(vtkObject sender, vtkObjectEventArgs e)
        {
            if (_IsOpenGLInitialized)
            {
                //Debug.WriteLine("renderer_EndEvt");

                // next commented line has the same effect as _GL.MakeCurrent()
                //renderWindowControl1.RenderWindow.MakeCurrent();
                //_GL.MakeCurrent();
                //_GL.MatrixMode(MatrixMode.Projection);
                //_GL.LoadIdentity();
                //_GL.Ortho(0, control.Width, control.Height, 0, -10, 10);
                //_GL.Clear(SharpGL.OpenGL.GL_DEPTH_BUFFER_BIT); // really needed ???
                //_GL.Disable(OpenGL.GL_LIGHTING);

                //// draw our own stuff
                //_GL.MatrixMode(MatrixMode.Modelview);
                //Draw2DGrid();
                //_GL.Flush();
                //_GL.Blit(_Context);
            }
        }

        Random r = new Random();


        internal event EventHandler<OpenGLEventArgs> OpenGLDrawEvent;

        IntPtr _Context;
        bool _IsOpenGLInitialized = false;


        void renderer_StartEvt(vtkObject sender, vtkObjectEventArgs e)
        {
        }

        void iren_LeftButtonPressEvt(vtkObject sender, vtkObjectEventArgs e)
        {
            if (sender == null)
            {
                return;
            }

            sbyte key = _iren.GetKeyCode();
            //if (key == 'p')
            {
                //Console.WriteLine(eventId);

                int[] pick = _iren.GetEventPosition();
                //regionEdges.Add(new Vector2(pick[0], pick[1]));

                Console.WriteLine("_iren.GetEventPosition = " + pick[0] + " " + pick[1]);

                _vtkWorldPointPicker.Pick((double)pick[0], (double)pick[1], 0.0, TopMostRenderer == null ? _renderer : TopMostRenderer);
                double[] pos = _vtkWorldPointPicker.GetPickPosition();

                if (Picked != null) 
                Picked(sender, new PickEventArg()
                {
                    Position = pos,
                    RendererPackage = this,
                    PickNormal = _vtkWorldPointPicker.GetPickNormal()
                });
            }
        }

        internal RendererPackage(RenderWindowControl renderWindowControl)
        {
           
            this.control = renderWindowControl;

            _renwin = renderWindowControl.RenderWindow;

            _renderer = renderWindowControl.RenderWindow.GetRenderers().GetFirstRenderer();

            _iren = renderWindowControl.RenderWindow.GetInteractor();

            vtkInteractorStyleTrackballCamera style = vtkInteractorStyleTrackballCamera.New();
            _iren.SetInteractorStyle(style);

            _iren.LeftButtonPressEvt += new vtkObject.vtkObjectEventHandler(iren_LeftButtonPressEvt);
            _iren.SetPicker(_vtkWorldPointPicker);

            //control.Resize += new EventHandler(control_Resize);
            this.control = control;

            cursor = new SpherePackage(_renderer);
            cursor.SetRadius(1);
            cursor.SetResolution(20, 20);
            cursor.VisibilityOff();

            Corner = new CornerAnnotationActorPackage(_renderer);
            //renderer.StartEvt += renderer_StartEvt;
            //renderer.EndEvt += renderer_EndEvt;
        }

        //public void UpdateOpenGLSequence()
        //{
        //    renderer.StartEvt -= new vtkObject.vtkObjectEventHandler(renderer_StartEvt);
        //    renderer.EndEvt -= new vtkObject.vtkObjectEventHandler(renderer_EndEvt);

        //    vtkRenderer topMost = vtkRenderer.SafeDownCast(RenderWindow.GetRenderers().GetItemAsObject(RenderWindow.GetRenderers().GetNumberOfItems() - 1));
        //    renderer.StartEvt += new vtkObject.vtkObjectEventHandler(renderer_StartEvt);
        //    topMost.EndEvt += new vtkObject.vtkObjectEventHandler(renderer_EndEvt);
        //}

        internal RendererPackage(vtkRenderer renderer, vtkRenderWindow renwin)
        {
            this._renderer = renderer;
            this._renwin = renwin;
        }

        internal RendererPackage()
        {
            _renwin = vtkRenderWindow.New();
            _renwin.SetSize(1024, 768);

            _renderer = vtkRenderer.New();
            _renwin.AddRenderer(_renderer);
            _iren = vtkRenderWindowInteractor.New();
            _iren.SetRenderWindow(_renwin);

            vtkInteractorStyleTrackballCamera style = vtkInteractorStyleTrackballCamera.New();
            _iren.SetInteractorStyle(style);

            // TODO FIX ME
            // iren.AddObserver((int)Kitware.VTK.vtkCommand.EventIds.LeftButtonPressEvent, new vtkDotNetCallback(MarchingCubeMousePress));

            _iren.SetPicker(_vtkWorldPointPicker);

            Corner = new CornerAnnotationActorPackage(_renderer);
        }

        private Control control;
        void control_Resize(object sender, EventArgs e)
        {
            int oldWidth = RenderWindow.GetSize()[0];
            int oldHeight = RenderWindow.GetSize()[1];
            //Console.WriteLine("Old Width = " + oldWidth);
            int newWidth = control.Width;
            int newHeight = control.Height;
            //Console.WriteLine("New Width = " + newWidth);

            double scale = (double)newWidth / (double)oldWidth;
            //Console.WriteLine(scale);
            double parellelScale;

            //if (this._renderer.GetActiveCamera().GetParallelProjection() == 0)
            {
                parellelScale = _renderer.GetActiveCamera().GetParallelScale();
            }
            //Console.WriteLine("Old Camera = " + renderer.GetActiveCamera());
            RenderWindow.SetSize(control.Width, control.Height);
            //Console.WriteLine("New Camera = " + renderer.GetActiveCamera());
            //if (renderer.GetActiveCamera().GetParallelProjection() == 0)
            ResetCamera();
            {
                _renderer.GetActiveCamera().SetParallelScale(parellelScale);
            }

            //renderer.GetActiveCamera().Zoom(scale);
            //renwin.Render();

            RefreshAll();
            //ResetCameraAndRefreshAll();

            //ResetCameraAndRefreshAll();

            //ResetCameraAndRefreshAll();
        }

        internal void RestCameraClippingRangeAndRefreshAll()
        {
            _renderer.ResetCameraClippingRange();
            RefreshAll();
        }

        internal void StartRefreshAll()
        {
            //if (renderer != null) renderer.Render();
            if (_renwin != null)
            {
                _renwin.SwapBuffersOff();
                _renwin.Render();
                _renwin.SwapBuffersOn();
            }

            //int depthPeelingWasUsed = renderer.GetLastRenderingUsedDepthPeeling();
            //Console.WriteLine("depthPeelingWasUsed  : " + depthPeelingWasUsed);
        }

        internal void FinishRefreshAll()
        {
            //if (renderer != null) renderer.Render();
            if (_renwin != null)
            {
                _renwin.Frame();
            }
        }

        internal void RefreshAll()
        {
            //if (renderer != null) renderer.Render();
            if (_renwin != null) _renwin.Render();

            //int depthPeelingWasUsed = renderer.GetLastRenderingUsedDepthPeeling();
            //Console.WriteLine("depthPeelingWasUsed  : " + depthPeelingWasUsed);
        }

        internal void Start()
        {
            //renWin.SetStereoTypeToCrystalEyes();
            _renwin.Render();
            //	SAVEIMAGE( renWin );

            _renderer.GetActiveCamera().SetUserTransform(vtkTransform.New());
            //renderer.GetActiveCamera().Dolly(90);
            //renderer.GetActiveCamera().Elevation(3.14 / 4);
            //renderer.GetActiveCamera().Pitch(-90);
            //renderer.GetActiveCamera().Azimuth(180);
            //renderer.GetActiveCamera().Roll(180);
            //renderer.GetActiveCamera().Dolly(90);

            //#if AdjustView
            _renderer.GetActiveCamera().Roll(90);
            _renderer.GetActiveCamera().Azimuth(90);
            _renderer.GetActiveCamera().Azimuth(90);
            _renderer.GetActiveCamera().Azimuth(90);
            _renderer.GetActiveCamera().Roll(90);
            _renderer.GetActiveCamera().Roll(90);
            _renderer.GetActiveCamera().Roll(90);
            //#endif

            Thread thread = new Thread(new ThreadStart(
                                                   delegate
                                                   {
                                                       _iren.Start();
                                                   }));
            thread.Start();
        }

        internal void Stop()
        {
            _iren.TerminateApp();

            //new Thread(new ThreadStart(delegate
            {
                _iren.Disable();
            }
            //)).Start();
            _iren.ExitEvent();
            _iren.TerminateApp();

            _iren.Dispose();

            _renwin.Dispose();
            _renderer.Dispose();
            _vtkWorldPointPicker.Dispose();
        }

        internal void ResetCameraClippingRangeAndRefreshAll()
        {
            ResetCameraClippingRange();
            RefreshAll();
        }

        internal void ResetCameraAndRefreshAll()
        {
            ResetCamera();
            RefreshAll();
        }

        internal void ResetCameraClippingRange()
        {
            _renderer.ResetCameraClippingRange();

            if (TopMostRenderer != null)
            {
                TopMostRenderer.ResetCameraClippingRange();
            }
        }

        internal void ResetCamera()
        {
            _renderer.ResetCameraClippingRange();
            _renderer.ResetCamera();
        }
        internal void ResetCamera(double[] bound)
        {
            _renderer.ResetCameraClippingRange();
            _renderer.ResetCamera(bound[0], bound[1], bound[2], bound[3], bound[4], bound[5]);
        }

        internal void AddActor(vtkActor actor)
        {
            _renderer.AddActor(actor);
        }

        internal void RemoveActor(vtkActor actor)
        {
            _renderer.RemoveActor(actor);
        }


        internal void InteractorStyleJoystickCamera()
        {
            _iren.SetInteractorStyle(vtkInteractorStyleJoystickCamera.New());
        }

        internal void InteractorStyleTrackballCamera()
        {
            _iren.SetInteractorStyle(vtkInteractorStyleTrackballCamera.New());
        }

        internal void InteractorStyleImage()
        {
            _iren.SetInteractorStyle(vtkInteractorStyleImage.New());
        }

        internal void InteractorStyleTrackballActor()
        {
            _iren.SetInteractorStyle(vtkInteractorStyleTrackballActor.New());
        }

        internal void InteractorStyleUser()
        {
            _iren.SetInteractorStyle(vtkInteractorStyleUser.New());
        }

        internal void InteractorStyleFlight()
        {
            _iren.SetInteractorStyle(vtkInteractorStyleFlight.New());
        }

        internal void InteractorStyleZoom()
        {
            _iren.SetInteractorStyle(vtkInteractorStyleImage.New());
        }

        internal double[] Pick(int x, int y)
        {
            WorldPointPicker.Pick(x, _renwin.GetSize()[1] - y, 0.0, Renderer);
            return WorldPointPicker.GetPickPosition();
        }

        internal double[] PickWithVtkPicker(int x, int y)
        {
            _iren.SetPicker(_vtkPicker);
            _vtkPicker.Pick(x, _renwin.GetSize()[1] - y, 0.0, Renderer);
            return _vtkPicker.GetPickPosition();
        }

        internal event EventHandler<PickEventArg> Picked;
        internal event EventHandler<ActorPickEventArg> ActorPicked;
        internal event EventHandler Rendererd;

        internal void RenderEvent(vtkObject obj, uint eventId, Object data, IntPtr clientdata)
        {
            if (Rendererd != null) Rendererd(obj, new EventArgs());
        }

        internal void ActorPickedMethod(vtkObject obj, uint eventId, Object data, IntPtr clientdata)
        {
            if (obj == null)
            {
                return;
            }

            int[] pick = _iren.GetEventPosition();

            Console.WriteLine("_iren.GetEventPosition = " + pick[0] + " " + pick[1]);
            int pickRet = _vtkPicker.Pick((double)pick[0], (double)pick[1], 0.0, _renderer);

            if (pickRet > 0)
            {
                vtkActor actor = _vtkPicker.GetActor();

                if (ActorPicked != null) { ActorPicked(obj, new ActorPickEventArg() { ActorPicked = actor }); }
            }
        }


        internal void MarchingCubeMousePress(vtkObject obj, uint eventId, Object data, IntPtr clientdata)
        {
            if (obj == null)
            {
                return;
            }

            sbyte key = _iren.GetKeyCode();
            //if (key == 'p')
            {
                //Console.WriteLine(eventId);

                int[] pick = _iren.GetEventPosition();
                //regionEdges.Add(new Vector2(pick[0], pick[1]));

                Console.WriteLine("_iren.GetEventPosition = " + pick[0] + " " + pick[1]);

                _vtkWorldPointPicker.Pick((double)pick[0], (double)pick[1], 0.0, _renderer);
                double[] pos = _vtkWorldPointPicker.GetPickPosition();

                if (Picked != null) Picked(obj, new PickEventArg() { Position = pos });
            }
        }

        internal void ScreenShot(string file)
        {
            vtkWindowToImageFilter windowToImageFilter = vtkWindowToImageFilter.New();
            windowToImageFilter.SetInput(_renwin);

            vtkBMPWriter writer = vtkBMPWriter.New();
            writer.SetFileName(file);
            writer.SetInput(windowToImageFilter.GetOutput());
            writer.Write();

            writer.Dispose();
            windowToImageFilter.Dispose();
        }

        internal System.Drawing.Bitmap ScreenShot()
        {
            vtkWindowToImageFilter windowToImageFilter = vtkWindowToImageFilter.New();
            windowToImageFilter.SetInput(_renwin);

            string file = "snapshot.jpg";
            vtkBMPWriter writer = vtkBMPWriter.New();
            writer.SetFileName(file);
            writer.SetInput(windowToImageFilter.GetOutput());
            writer.Write();

            writer.Dispose();
            windowToImageFilter.Dispose();

            System.Drawing.Bitmap bitmap =(System.Drawing.Bitmap) System.Drawing.Bitmap.FromFile(file);
            File.Delete(file);
            return bitmap;



        }


        internal System.Drawing.Bitmap ScreenShotToFile(string saveFile)
        {
            vtkWindowToImageFilter windowToImageFilter = vtkWindowToImageFilter.New();
            
            windowToImageFilter.SetInput(_renwin);

            vtkBMPWriter writer = vtkBMPWriter.New();
            writer.SetFileName(saveFile);
            writer.SetInput(windowToImageFilter.GetOutput());
            writer.Write();

            writer.Dispose();
            windowToImageFilter.Dispose();

            System.Drawing.Bitmap bitmap = (System.Drawing.Bitmap)System.Drawing.Bitmap.FromFile(saveFile);
            return bitmap;

        }

        private vtkMatrix4x4 bakCam = vtkMatrix4x4.New();
        internal void BackUpCamera()
        {
            VTKUtil.CopyMatrix(_renderer.GetActiveCamera().GetViewTransformMatrix(), bakCam);

            Console.WriteLine(bakCam);
        }

        internal void RestoreCamera()
        {
            vtkTransform transform = vtkTransform.New();
            transform.SetMatrix(bakCam);

            VTKUtil.CopyMatrix(bakCam, _renderer.GetActiveCamera().GetViewTransformMatrix());
        }

        internal void SetUpCamera(double[] matrix)
        {
            //vtkHomogeneousTransform transform = new vtkHomogeneousTransform();
            //transform.GetMatrix().SetElement();
            //renderer.GetActiveCamera().SetUserTransform(new vtkHomogeneousTransform());
        }

        internal void ParallelProjectionOn()
        {
            for (int i = 0; i < RenderWindow.GetRenderers().GetNumberOfItems(); ++i)
            {
                vtkRenderer.SafeDownCast(RenderWindow.GetRenderers().GetItemAsObject(i)).GetActiveCamera().ParallelProjectionOn();
            }
        }

        internal void ParallelProjectionOff()
        {
            for (int i = 0; i < RenderWindow.GetRenderers().GetNumberOfItems(); ++i)
            {
                vtkRenderer.SafeDownCast(RenderWindow.GetRenderers().GetItemAsObject(i)).GetActiveCamera().ParallelProjectionOff();
            }
            //renderer.GetActiveCamera().ParallelProjectionOff();
        }

        internal void JumpTo(double[] position)
        {
            Renderer.GetActiveCamera().SetFocalPoint(position[0], position[1], position[2]);
            ResetCamera();
            RefreshAll();
        }

        #region IDisposable Members

        internal void Dispose()
        {
            if (_iren != null)
            {
                _iren.RemoveAllObservers();
                _iren.ExitCallback();
                _iren.Disable();
            }
            VTKUtil.DisposeVtkObject(_vtkWorldPointPicker);
            VTKUtil.DisposeVtkObject(_renwin);
            VTKUtil.DisposeVtkObject(_iren);
            VTKUtil.DisposeVtkObject(_renderer);
            VTKUtil.DisposeVtkObject(_vtkWorldPointPicker);
        }

        #endregion

        Dictionary<string, XmlPolyDataPackage> polyDataDict = new Dictionary<string, XmlPolyDataPackage>();
        Dictionary<string, SpherePackage> sphereDict = new Dictionary<string, SpherePackage>();
        Dictionary<string, CrossPackage> crossDict = new Dictionary<string, CrossPackage>();
        Dictionary<string, LinePackage> linesDict = new Dictionary<string, LinePackage>();
        Dictionary<string, TubePackage> tubesDict = new Dictionary<string, TubePackage>();
        Dictionary<string, List<LinePackage>> polylinesDict = new Dictionary<string, List<LinePackage>>();

        internal void ClearSetPolylines()
        {
            foreach (KeyValuePair<string, List<LinePackage>> keyValuePair in polylinesDict)
            {
                foreach (LinePackage linePackage in keyValuePair.Value)
                {
                    linePackage.RemoveMe();
                }
            }

            polylinesDict.Clear();
        }

        internal void SetPolyline(string name, List<double[]> points, double[] color)
        {
            SetPolyline(name, points);
            foreach (var item in polylinesDict[name])
            {
                item.SetColor(color[0], color[1], color[2]);
            }
        }

        internal void SetPolyline(string name, List<double[]> points, double[] color, vtkRenderer renderer)
        {
            SetPolyline(name, points, renderer);
            foreach (var item in polylinesDict[name])
            {
                item.SetColor(color[0], color[1], color[2]);
            }
        }

        internal void SetPolyline(string name, List<double[]> points)
        {
            if (polylinesDict.ContainsKey(name))
            {
                foreach (var item in polylinesDict[name])
                {
                    item.RemoveMe();
                }

                polylinesDict[name].Clear();
            }

            polylinesDict[name] = new List<LinePackage>();

            for (int i = 0; i < points.Count - 1; ++i)
            {
                double[] p1 = points[i];
                double[] p2 = points[i + 1];

                LinePackage linePackage = new LinePackage(this, p1, p2);

                polylinesDict[name].Add(linePackage);
            }
        }


        internal void SetPolyline(string name, List<double[]> points, vtkRenderer renderer)
        {
            if (polylinesDict.ContainsKey(name))
            {
                foreach (var item in polylinesDict[name])
                {
                    item.RemoveMe();
                }

                polylinesDict[name].Clear();
            }

            polylinesDict[name] = new List<LinePackage>();

            for (int i = 0; i < points.Count - 1; ++i)
            {
                double[] p1 = points[i];
                double[] p2 = points[i + 1];

                LinePackage linePackage = new LinePackage(renderer);
                linePackage.SetPosition(p1, p2);

                polylinesDict[name].Add(linePackage);
            }
        }

        internal void SetTube(string n1, string n2)
        {
            if (!tubesDict.ContainsKey(n1 + "-" + n2))
            {
                tubesDict[n1 + "-" + n2] = new TubePackage(TopMostRenderer ?? this._renderer);
            }

            //tubesDict[n1 + "-" + n2].SetPosition(GetSpherePackage(n1).GetPosition(), GetSpherePackage(n2).GetPosition());
            //tubesDict[n1 + "-" + n2].SetColor(1, 0, 0);
        }

        private CrossPackage cameraPositionCross = null;

        internal void SetCameraLine(vtkMatrix4x4 matrix, double length)
        {
            if (cameraPositionCross == null)
            {
                cameraPositionCross = new CrossPackage(20, _renderer);
            }

            cameraPositionCross.SetPosition(VTKUtil.MultiPoint(new double[] { 0, 0, 0 }, matrix));

            SetCameraLine(
                VTKUtil.MultiPoint(new double[] { 0, 0, 0 }, matrix),
                VTKUtil.MultiPoint(new double[] { 0, 0, length }, matrix));
            SetCameraViewUp(
                VTKUtil.MultiPoint(new double[] { 0, 0, 0 }, matrix),
                VTKUtil.MultiPoint(new double[] { 0, length / 3, 0 }, matrix));
        }

        internal void SetCameraLine(double[] cameraPosition, double[] focalPosition)
        {
            const string privateCameraposition = "private_cameraposition";
            const string privateFocalposition = "private_focalposition";

            SetPosition(privateCameraposition, cameraPosition);
            SetPosition(privateFocalposition, focalPosition);

            //SetLine(privateCameraposition, privateFocalposition);
        }

        internal void SetCameraViewUp(double[] cameraPosition, double[] viewUp)
        {
            const string privateCameraposition = "private_cameraposition";
            const string privateFocalposition = "private_viewupposition";

            SetPosition(privateCameraposition, cameraPosition);
            SetPosition(privateFocalposition, viewUp);

            //SetLine(privateCameraposition, privateFocalposition);
        }

        //internal void SetLine(string n1, string n2)
        //{
        //    if (!linesDict.ContainsKey(n1 + "-" + n2))
        //    {
        //        linesDict[n1 + "-" + n2] = new LinePackage(this);
        //    }

        //    linesDict[n1 + "-" + n2].SetPosition(GetSpherePackage(n1).GetPosition(), GetSpherePackage(n2).GetPosition());
        //    linesDict[n1 + "-" + n2].SetColor(1, 0, 0);
        //}

        //internal void SetLine(double[] point1, string n1, double[] point2, string n2)
        //{
        //    SetPosition(n1, point1);
        //    SetPosition(n2, point2);

        //    if (!linesDict.ContainsKey(n1 + "-" + n2))
        //    {
        //        linesDict[n1 + "-" + n2] = new LinePackage(this);
        //    }

        //    linesDict[n1 + "-" + n2].SetPosition(GetSpherePackage(n1).GetPosition(), GetSpherePackage(n2).GetPosition());
        //    linesDict[n1 + "-" + n2].SetColor(1, 0, 0);
        //}

        internal SpherePackage SetPosition(string name, double[] pos, double size, int resolution)
        {
            if (!sphereDict.ContainsKey(name))
            {
                sphereDict[name] = new SpherePackage(TopMostRenderer ?? _renderer);
            }

            sphereDict[name].SetPosition(pos);
            sphereDict[name].SetName(name);
            sphereDict[name].SetRadius(size);
            sphereDict[name].SetResolution(resolution, resolution);

            sphereDict[name].SetName("");

            return sphereDict[name];
        }

        //internal void SetMiddlePosition(string p1Name, string p2Name)
        //{
        //    double[] p1 = GetSpherePackage(p1Name).GetPosition();
        //    double[] p2 = GetSpherePackage(p2Name).GetPosition();

        //    cursor.SetPosition(
        //        new double[]
        //            {
        //                (p1[0]+p2[0])/2,
        //                (p1[1]+p2[1])/2,
        //                (p1[2]+p2[2])/2
        //            });
        //    RefreshAll();
        //}

        internal void SetPosition2CursorAndRefresh(string name)
        {
            //SetPosition(name, CursorPosition);
            RefreshAll();
        }

        internal void SetPositionAndRefreshAll(string name, double[] pos)
        {
            SetPosition(name, pos);
            RefreshAll();
        }
        internal CrossPackage SetCross(string name, double[] pos, int[] color)
        {
            if (!crossDict.ContainsKey(name))
            {
                crossDict[name] = new CrossPackage(5, TopMostRenderer ?? _renderer);
            }

            crossDict[name].SetPosition(pos);
            crossDict[name].SetColor(color[0], color[1], color[2]);

            return crossDict[name];
        }

        internal SpherePackage SetPosition(string name, double[] pos)
        {
            if (!sphereDict.ContainsKey(name))
            {
                sphereDict[name] = new SpherePackage(TopMostRenderer ?? _renderer);
            }

            sphereDict[name].SetPosition(pos);
            sphereDict[name].SetName(name);
            //sphereDict[name].SetTextAsTopMost();

            return sphereDict[name];
        }

        internal bool ContainsSpherePackage(string name)
        {
            return sphereDict.ContainsKey(name);
        }

        //internal double[] GetSpherePosition(string name)
        //{
        //    if (!sphereDict.ContainsKey(name))
        //    {
        //        return null;
        //    }
        //    return GetSpherePackage(name).GetPosition();
        //}

        internal SpherePackage GetSpherePackage(string name)
        {
            if (!sphereDict.ContainsKey(name))
            {
                return null;
            }

            return sphereDict[name];
        }

        internal LinePackage GetLinePackage(string name)
        {
            if (!linesDict.ContainsKey(name))
            {
                return null;
            }

            return linesDict[name];
        }

        internal TubePackage GetTube(string name)
        {
            if (!tubesDict.ContainsKey(name))
            {
                return null;
            }

            return tubesDict[name];
        }

        internal void RemovePositionByStart(string start)
        {
            List<string> names = new List<string>();
            foreach (KeyValuePair<string, SpherePackage> spherePackage in sphereDict)
            {
                if (spherePackage.Key.StartsWith(start))
                {
                    names.Add(spherePackage.Key);
                }
            }
            foreach (string name in names)
            {
                RemovePosition(name);
            }
        }

        internal void RemovePosition(string name)
        {
            if (!sphereDict.ContainsKey(name))
            {
                return;
            }

           // sphereDict[name].RemoveMe();
            sphereDict.Remove(name);
        }

        internal void SetOpacity(string name, double opacity)
        {
            if (!sphereDict.ContainsKey(name))
            {
                sphereDict[name] = new SpherePackage(_renderer);
            }

            sphereDict[name].SetOpacity((float)opacity);
        }

        internal void SetRadius(string name, double radius)
        {
            if (!sphereDict.ContainsKey(name))
            {
                sphereDict[name] = new SpherePackage(_renderer);
            }

            sphereDict[name].SetRadius(radius);
        }

        internal void SetResolution(string name, int r)
        {
            if (!sphereDict.ContainsKey(name))
            {
                sphereDict[name] = new SpherePackage(_renderer);
            }

            sphereDict[name].SetResolution(r, r);
        }

        internal void SetRadius(int radius)
        {
            foreach (var item in sphereDict)
            {
                item.Value.SetRadius(radius);
            }
        }

        public vtkRenderer TopMostRenderer;

        public void AddTopMost()
        {
            if (_renderer.GetLayer() > 0)
            {
                return;
            }

            if (_renderer.GetRenderWindow().GetRenderers().GetNumberOfItems() == 1)
            {
                TopMostRenderer = vtkRenderer.New(); // 2d actor        
                TopMostRenderer.SetLayer(1); // top layer    
                TopMostRenderer.InteractiveOn();

                _renderer.SetLayer(0);
                _renderer.GetRenderWindow().SetNumberOfLayers(2);
                _renderer.GetRenderWindow().AddRenderer(TopMostRenderer);

                //ren1.SetActiveCamera(_aRender.GetActiveCamera());

                _renderer.SetActiveCamera(TopMostRenderer.GetActiveCamera());
                //TopMostRenderer.SetActiveCamera(renderer.GetActiveCamera());

                _renderer.InteractiveOff();
            }
        }

        internal vtkLight Light
        {
            get
            {
                return vtkLight.SafeDownCast(_renderer.GetLights().GetItemAsObject(0));
            }
        }

        internal void LightPositionalOn()
        {
            Light.SetConeAngle(Camera.GetViewAngle());
            Light.SetConeAngle(80);
            Light.PositionalOn();
        }

        internal void LightPositionalOff()
        {
            Light.PositionalOff();
        }

        internal void UpdateBrightLight(int attenuationValue)
        {
            UpdateBrightLight(_renderer.GetActiveCamera(), _renderer, attenuationValue, _renwin);
        }

        private static void UpdateBrightLight(vtkCamera aCamera, vtkRenderer _renderer, int attenuationValue, vtkRenderWindow _renwin)
        {
#if! LightZLF
            _renderer.LightFollowCameraOff();
            _renderer.AutomaticLightCreationOff();
            _renderer.RemoveAllLights();

            vtkLight light = vtkLight.New();
            light.SetLightTypeToCameraLight();
            //light.SetLightTypeToSceneLight();

            double[] lightPosition = aCamera.GetPosition();
            double[] lightFocalPosition = aCamera.GetPosition();

            lightPosition[0] -= aCamera.GetDirectionOfProjection()[0] * 10;
            lightPosition[1] -= aCamera.GetDirectionOfProjection()[1] * 10;
            lightPosition[2] -= aCamera.GetDirectionOfProjection()[2] * 10;

            light.SetPosition(lightPosition[0], lightPosition[1], lightPosition[2]);
            light.SetFocalPoint(aCamera.GetFocalPoint()[0], aCamera.GetFocalPoint()[1], aCamera.GetFocalPoint()[2]);

            lightFocalPosition[0] = (aCamera.GetFocalPoint()[0] + lightFocalPosition[0]) / 2;
            lightFocalPosition[1] = (aCamera.GetFocalPoint()[1] + lightFocalPosition[1]) / 2;
            lightFocalPosition[2] = (aCamera.GetFocalPoint()[2] + lightFocalPosition[2]) / 2;

            //light.SetFocalPoint(lightFocalPosition[0], lightFocalPosition[1], lightFocalPosition[2]);
            //light.SetConeAngle(30);
            //light.SetExponent(1);
            light.SetConeAngle(80);
            //light.SetLightTypeToHeadlight();
            light.SetIntensity(1);
            //light.SetIntensity(1);

            light.SetColor(195f / 255f, 89f / 255f, 101f / 255f);
            //light.SetLightTypeToSceneLight();
            light.SetAttenuationValues(0, 0, 0.0001 * attenuationValue);
            //light.SetAmbientColor(0, 1, 0);
            //light.SetDiffuseColor(1, 0, 0);
            //light.SetSpecularColor(1, 1, 1);

            //light.PositionalOn();

            _renderer.AddLight(light);
#endif

            _renwin.Render();
        }
        internal void UpdateLight(int attenuationValue)
        {
            UpdateLight(_renderer.GetActiveCamera(), _renderer, attenuationValue, _renwin);
        }

        private static void UpdateLight(vtkCamera aCamera, vtkRenderer _renderer, int attenuationValue, vtkRenderWindow _renwin)
        {
#if! LightZLF
            _renderer.LightFollowCameraOff();
            _renderer.AutomaticLightCreationOff();
            _renderer.RemoveAllLights();

            vtkLight light = vtkLight.New();
            light.SetLightTypeToCameraLight();

            double[] lightPosition = aCamera.GetPosition();
            double[] lightFocalPosition = aCamera.GetPosition();

            lightPosition[0] -= aCamera.GetDirectionOfProjection()[0] * 10;
            lightPosition[1] -= aCamera.GetDirectionOfProjection()[1] * 10;
            lightPosition[2] -= aCamera.GetDirectionOfProjection()[2] * 10;

            light.SetPosition(lightPosition[0], lightPosition[1], lightPosition[2]);
            light.SetFocalPoint(aCamera.GetFocalPoint()[0], aCamera.GetFocalPoint()[1], aCamera.GetFocalPoint()[2]);

            lightFocalPosition[0] = (aCamera.GetFocalPoint()[0] + lightFocalPosition[0]) / 2;
            lightFocalPosition[1] = (aCamera.GetFocalPoint()[1] + lightFocalPosition[1]) / 2;
            lightFocalPosition[2] = (aCamera.GetFocalPoint()[2] + lightFocalPosition[2]) / 2;

            //light.SetFocalPoint(lightFocalPosition[0], lightFocalPosition[1], lightFocalPosition[2]);
            //light.SetConeAngle(30);
            //light.SetExponent(1);
            //light.SetConeAngle(100);
            //light.SetLightTypeToHeadlight();
            //light.SetIntensity(0.5);
            //light.SetIntensity(1);

            //light.SetColor(195f / 255f, 89f / 255f, 101f / 255f);
            //light.SetLightTypeToSceneLight();
            //light.SetAttenuationValues(0, 0, 0.0001 * attenuationValue);
            //light.SetAmbientColor(0, 1, 0);
            //light.SetDiffuseColor(1, 0, 0);
            //light.SetSpecularColor(1, 1, 1);

            light.PositionalOn();

            _renderer.AddLight(light);
#endif

            _renwin.Render();
        }

        DateTime lastUpdateTime = DateTime.Now;
        private WatermarkPackage _watermarkPackage;
        //private WatermarkPackage _watermarkPackage;

        //internal void WatermarkOn()
        //{
        //    if (_watermarkPackage != null)
        //    {
        //        _watermarkPackage.On();
        //    }
        //}

        //internal void WatermarkOff()
        //{
        //    if (_watermarkPackage != null)
        //    {
        //        _watermarkPackage.Off();
        //    }
        //}

        internal void UpdateFPS()
        {
            if (Corner != null)
            {
                double timeInSeconds = (DateTime.Now - lastUpdateTime).TotalMilliseconds;
                lastUpdateTime = DateTime.Now;
                double fps = 1000.0 / timeInSeconds;
                Corner.SetRightTop(string.Format("FPS: {0:F0}", fps));
            }
            //std::cout << "FPS: " << fps << std::endl;
        }

        internal void UpdateCamera(vtkCamera mainViewCamera)
        {
            //return;

            vtkCamera OrientationEnumViewCamera = Camera;

            double[] mainViewCameraPosition = mainViewCamera.GetPosition();
            double[] mainViewFocalPosition = mainViewCamera.GetFocalPoint();
            double[] mainViewViewUp = mainViewCamera.GetViewUp();

            OrientationEnumViewCamera.SetPosition(mainViewCameraPosition[0], mainViewCameraPosition[1], mainViewCameraPosition[2]);
            OrientationEnumViewCamera.SetFocalPoint(mainViewFocalPosition[0], mainViewFocalPosition[1], mainViewFocalPosition[2]);
            OrientationEnumViewCamera.SetViewUp(mainViewViewUp[0], mainViewViewUp[1], mainViewViewUp[2]);
            OrientationEnumViewCamera.ComputeViewPlaneNormal();
            OrientationEnumViewCamera.ComputeProjAndViewParams();
            OrientationEnumViewCamera.OrthogonalizeViewUp();

            _renderer.ResetCameraClippingRange();
            //_renderer.ResetCamera();
        }

        internal void FlyTo(double[] focal)
        {
            double[] directionOfProjection = Camera.GetDirectionOfProjection();
            double distance = Camera.GetDistance();
            Camera.SetFocalPoint(focal[0], focal[1], focal[2]);
            Camera.SetPosition(
                focal[0] - distance * directionOfProjection[0],
                focal[1] - distance * directionOfProjection[1],
                focal[2] - distance * directionOfProjection[2]);
        }

        internal void ClearSpheres()
        {
            Dictionary<string, SpherePackage> spherePackages = new Dictionary<string, SpherePackage>(sphereDict);
            foreach (KeyValuePair<string, SpherePackage> spherePackage in spherePackages)
            {
                RemovePosition(spherePackage.Key);
            }
        }

        internal void ClearAllPolyData()
        {
            Dictionary<string, XmlPolyDataPackage> xmlPolyDataPackages = new Dictionary<string, XmlPolyDataPackage>(polyDataDict);
            foreach (KeyValuePair<string, XmlPolyDataPackage> spherePackage in xmlPolyDataPackages)
            {
                string name = spherePackage.Key;
                //polyDataDict[name].RemoveMe();
                polyDataDict.Remove(name);
            }
        }
    }

    public class PickEventArg : EventArgs
    {
        public RendererPackage RendererPackage;
        public double[] Position;
        public double[] PickNormal;
    }

    internal class ImageWorldPickEventArg : EventArgs
    {
        internal double[] Position;
        internal int[] ImagePosition;
    }

    internal class ActorPickEventArg : EventArgs
    {
        internal vtkActor ActorPicked;
    }

    internal class OpenGLEventArgs : EventArgs
    {
        internal RendererPackage RendererPackage { get; set; }
    }
}
