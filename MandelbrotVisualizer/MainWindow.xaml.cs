using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;

namespace MandelbrotVisualizer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private GLControl glc;
        private bool glInitDone;
        private int width, height;
        private int shaderProgram;
        private int rectangleVao;
        private int indicesCount;
        private Matrix4 viewMatrix;
        private Matrix4 projMatrix;
        private Vector2d baseCenterPoint = new Vector2d(-0.5, 0);
        private Vector2d currentCenterPoint = new Vector2d(-0.5, 0);
        private double baseScale = 1.2;
        private double currentScale = 1.2;
        private float renderingResolutionScale = 0.3f;
        private MousePosition oldMousePosition;
        private int fbo;
        private int colorRbo;
        private Timer loopTimer;

        public MainWindow()
        {
            glInitDone = false;
            InitializeComponent();
            InitializerGlComponent();
            loopTimer = new Timer();
            loopTimer.Interval = 1;
            loopTimer.Tick += LoopTimer_Tick;
            loopTimer.Start();
        }

        private void LoopTimer_Tick(object sender, EventArgs e)
        {
            //glc?.Invalidate();
            if (renderingResolutionScale <= 1)
            {
                glc?.Invalidate();
                renderingResolutionScale += 0.05f;
            }
        }

        private void InitializerGlComponent()
        {
            glc = new GLControl();
            Toolkit.Init();
            glc.Paint += Glc_Paint;
            glc.Resize += Glc_Resize;
            glc.MouseMove += Glc_MouseMove;
            glc.MouseUp += (s, e) => ClearMousePosition();
            glc.MouseLeave += (s, e) => ClearMousePosition();
            wfHost.Child = glc;
            glc.CreateControl();
        }

        private void Glc_Resize(object sender, EventArgs e)
        {
            width = glc.Width;
            height = glc.Height;
            renderingResolutionScale = 0.3f;
        }

        private void Glc_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        {
            if (glInitDone)
                GlDraw();
            else
            {
                GlInit();
                glInitDone = true;
            }
        }

        private void GlInit()
        {
            GL.ClearColor(1f, 0f, 0f, 1f);

            string vertexShaderSource = ReadShaderString("Mandelbrot.vert");
            string fragmentShaderSource = ReadShaderString("Mandelbrot.frag");

            int vertexShaderHandle = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertexShaderHandle, vertexShaderSource);
            GL.CompileShader(vertexShaderHandle);
            int fragmentShaderHandle = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShaderHandle, fragmentShaderSource);
            GL.CompileShader(fragmentShaderHandle);

            string vinfo = GL.GetShaderInfoLog(vertexShaderHandle);
            string finfo = GL.GetShaderInfoLog(fragmentShaderHandle);

            shaderProgram = GL.CreateProgram();
            GL.AttachShader(shaderProgram, vertexShaderHandle);
            GL.AttachShader(shaderProgram, fragmentShaderHandle);
            GL.BindAttribLocation(shaderProgram, 0, "inPosition");
            GL.LinkProgram(shaderProgram);
            GL.UseProgram(shaderProgram);

            GL.CreateRenderbuffers(1, out colorRbo);
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, colorRbo);
            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.Rgb8, (int)SystemParameters.PrimaryScreenWidth, (int)SystemParameters.PrimaryScreenHeight);
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, 0);

            GL.GenFramebuffers(1, out fbo);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, fbo);
            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, RenderbufferTarget.Renderbuffer, colorRbo);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

            projMatrix = Matrix4.CreateOrthographic(2f, 2f, 0.1f, 10f);
            viewMatrix = Matrix4.LookAt(new Vector3(0f, 0f, 6f), new Vector3(0f, 0f, 0f), Vector3.UnitY);
        }

        private void GlDraw()
        {
            //first render on the scaled resolution framebuffer
            int scaledWidth = (int)(width * Math.Min(renderingResolutionScale, 1.0f));
            int scaledHeight = (int)(height * Math.Min(renderingResolutionScale, 1.0f));
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, fbo);
            GL.Viewport(0, 0, scaledWidth, scaledHeight);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            //draw rectangle
            if (rectangleVao < 1)
                CreateRectangle();
            Matrix4 mvpMatrix = viewMatrix * projMatrix;//model matrix is identity
            GL.UniformMatrix4(GL.GetUniformLocation(shaderProgram, "mvpMatrix"), false, ref mvpMatrix);
            GL.Uniform2(GL.GetUniformLocation(shaderProgram, "centerPoint"), currentCenterPoint.X, currentCenterPoint.Y);
            GL.Uniform1(GL.GetUniformLocation(shaderProgram, "unitSize"), currentScale);
            GL.Uniform1(GL.GetUniformLocation(shaderProgram, "aspectRatio"), (double)width / height);
            GL.BindVertexArray(rectangleVao);
            GL.DrawElements(PrimitiveType.Triangles, indicesCount, DrawElementsType.UnsignedInt, IntPtr.Zero);
            GL.BindVertexArray(0);

            //copy data to main framebuffer
            GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, 0);
            GL.Viewport(0, 0, width, height);
            GL.BlitFramebuffer(0, 0, scaledWidth, scaledHeight, 0, 0, width, height, ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit, BlitFramebufferFilter.Nearest);

            glc.SwapBuffers();
        }

        private void CreateRectangle()
        {
            Vector3[] positions =
            {
                new Vector3(-1, 1, -1f),
                new Vector3(-1, -1, -1f),
                new Vector3(1, -1, -1f),
                new Vector3(1, 1, -1f)
            };
            int[] indices = { 0, 1, 2, 0, 2, 3 };
            indicesCount = indices.Length;
            int posVbo, ebo;
            GL.CreateBuffers(1, out posVbo);
            GL.BindBuffer(BufferTarget.ArrayBuffer, posVbo);
            GL.BufferData(BufferTarget.ArrayBuffer, Vector3.SizeInBytes * positions.Length, positions, BufferUsageHint.StaticDraw);
            GL.CreateBuffers(1, out ebo);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, sizeof(int) * indices.Length, indices, BufferUsageHint.StaticDraw);

            GL.CreateVertexArrays(1, out rectangleVao);
            GL.BindVertexArray(rectangleVao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, posVbo);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, true, Vector3.SizeInBytes, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
            GL.BindVertexArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
        }

        private void Glc_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if(e.Button == MouseButtons.Left)
            {
                MousePosition newMousePosition = new MousePosition { X = e.X, Y = e.Y };
                if(oldMousePosition != null)
                {
                    int xMovement = oldMousePosition.X - newMousePosition.X;
                    int yMovement = newMousePosition.Y - oldMousePosition.Y;
                    double pixelSize = 2 * currentScale / height;
                    currentCenterPoint.X += xMovement * pixelSize;
                    currentCenterPoint.Y += yMovement * pixelSize;
                    renderingResolutionScale = 0.3f;
                }
                oldMousePosition = newMousePosition;
            }
        }

        private string ReadShaderString(string fileName)
        {
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("MandelbrotVisualizer.Shaders." + fileName))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        private void ClearMousePosition()
        {
            oldMousePosition = null;
        }

        public class MousePosition
        {
            public int X { get; set; }
            public int Y { get; set; }
        }
    }
}
