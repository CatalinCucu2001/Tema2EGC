using System;
using System.Drawing;
using System.Drawing.Printing;
using System.Security.Policy;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace EGCTema2
{
    class ImmediateMode : GameWindow
    {
        private const int XYZ_SIZE = 75;

        private Vector3 posCube = new Vector3(0, 0, 0);
        private Vector3 posEye = new Vector3(30, 30, 30);
        private double angleEye = 0;
        private double radius = 0;

        private int mouseState = 0; // 0 - released, 1 - click, 2 - hold;
        private Vector2 mousePos;

        public ImmediateMode() : base(800, 600, new GraphicsMode(32, 24, 0, 8))
        {
            VSync = VSyncMode.On;

            Console.WriteLine("OpenGl versiunea: " + GL.GetString(StringName.Version));
            Title = "OpenGl versiunea: " + GL.GetString(StringName.Version) + " (mod imediat)";

        }

        /**Setare mediu OpenGL și încarcarea resurselor (dacă e necesar) - de exemplu culoarea de
           fundal a ferestrei 3D.
           Atenție! Acest cod se execută înainte de desenarea efectivă a scenei 3D. */
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            GL.ClearColor(Color.DarkGray);
            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Less);
            GL.Hint(HintTarget.PolygonSmoothHint, HintMode.Nicest);

             
        }

        /**Inițierea afișării și setarea viewport-ului grafic. Metoda este invocată la redimensionarea
           ferestrei. Va fi invocată o dată și imediat după metoda ONLOAD()!
           Viewport-ul va fi dimensionat conform mărimii ferestrei active (cele 2 obiecte pot avea și mărimi 
           diferite). */
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            GL.Viewport(0, 0, Width, Height);

            double aspect_ratio = Width / (double)Height;

            Matrix4 perspective = Matrix4.CreatePerspectiveFieldOfView(MathHelper.PiOver4, (float)aspect_ratio, 1, 100);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref perspective);

            Matrix4 lookat = Matrix4.LookAt(posEye.X, posEye.Y, posEye.Z, posCube.X, posCube.Y, posCube.Z, 0, 1, 0);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref lookat);

            angleEye = Math.Tan(posEye.Z / posEye.X);



        }

        /** Secțiunea pentru "game logic"/"business logic". Tot ce se execută în această secțiune va fi randat
            automat pe ecran în pasul următor - control utilizator, actualizarea poziției obiectelor, etc. */
        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            KeyboardState keyboard = Keyboard.GetState();
            MouseState mouse = Mouse.GetState();

            if (mouse.LeftButton == ButtonState.Released)
            {
                mouseState = 0;
            }
            else if (mouse.LeftButton == ButtonState.Pressed && mouseState == 0)
            {
                mouseState++;
                mousePos = new Vector2(mouse.X, mouse.Y);
            }
            else if (mouse.LeftButton == ButtonState.Pressed)
            {
                var delta = mousePos.Y - mouse.Y;

                if (delta != 0)
                {
                    if (delta < 0)
                    {
                        delta = 1 - 1 / Math.Max((100 + delta), 10);
                    }
                    else
                    {
                        delta = 1 + delta/(delta + 1000);
                    }

                    Console.WriteLine(delta);

                    posEye.X *= delta;
                    posEye.Y *= delta;
                    posEye.Z *= delta;
                    radius *= Math.Sqrt(Math.Pow(posEye.X, 2) + Math.Pow(posEye.Y, 2));

                    Matrix4 lookat = Matrix4.LookAt(posEye.X, posEye.Y, posEye.Z, posCube.X, posCube.Y, posCube.Z, 0, 1, 0);
                    GL.MatrixMode(MatrixMode.Modelview);
                    GL.LoadMatrix(ref lookat);

                    mousePos = new Vector2(mouse.X, mouse.Y);

                    Console.WriteLine("Radius: " + radius);

                }


            }


            if (keyboard[Key.Escape])
            {
                Exit();
            }

            if (keyboard[Key.Up])
            {
                posEye = new Vector3(posEye.X, posEye.Y + 1, posEye.Z);
            }

            if (keyboard[Key.Down])
            {
                posEye = new Vector3(posEye.X, posEye.Y - 1, posEye.Z);
            }
            
            if (keyboard[Key.Left])
            {
                angleEye -= 0.05;
                posEye = new Vector3((float)(radius * (float)Math.Cos(angleEye)), posEye.Y, (float)(radius * (float)Math.Sin(angleEye)));
            }

            if (keyboard[Key.Right])
            {
                angleEye += 0.05;
                posEye = new Vector3((float)(radius * (float)Math.Cos(angleEye)), posEye.Y, (float)(radius * (float)Math.Sin(angleEye)));
            }


            Console.WriteLine(angleEye + " " + Math.Cos(angleEye) + " " + Math.Sin(angleEye));


        }

        /** Secțiunea pentru randarea scenei 3D. Controlată de modulul logic din metoda ONUPDATEFRAME().
            Parametrul de intrare "e" conține informatii de timing pentru randare. */
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.Clear(ClearBufferMask.DepthBufferBit);

            Matrix4 lookat = Matrix4.LookAt(posEye.X, posEye.Y, posEye.Z, posCube.X, posCube.Y, posCube.Z, 0, 1, 0);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref lookat);


            //DrawAxes();

            DrawObjects();



            // Se lucrează în modul DOUBLE BUFFERED - câtă vreme se afișează o imagine randată, o alta se randează în background apoi cele 2 sunt schimbate...
            SwapBuffers();
        }

        private void DrawAxes()
        {

            //GL.LineWidth(3.0f);

            // Desenează axa Ox (cu roșu).
            GL.Begin(PrimitiveType.Lines);
            GL.Color3(Color.Red);
            GL.Vertex3(0, 0, 0);
            GL.Vertex3(XYZ_SIZE, 0, 0);
            GL.End();

            // Desenează axa Oy (cu galben).
            GL.Begin(PrimitiveType.Lines);
            GL.Color3(Color.Yellow);
            GL.Vertex3(0, 0, 0);
            GL.Vertex3(0, XYZ_SIZE, 0); ;
            GL.End();

            // Desenează axa Oz (cu verde).
            GL.Begin(PrimitiveType.Lines);
            GL.Color3(Color.Green);
            GL.Vertex3(0, 0, 0);
            GL.Vertex3(0, 0, XYZ_SIZE);
            GL.End();
        }

        private void DrawObjects()
        {
            
            DrawCube(10);

        }

        private void DrawCube(float x)
        {
            radius = x * 5;

            posCube = new Vector3(x / 2, x / 2, x / 2);

            GL.Begin(PrimitiveType.QuadStrip);
            GL.Color3(Color.Red);
            GL.Vertex3(0, 0, 0);
            GL.Vertex3(x, 0, 0);
            GL.Vertex3(0, 0, x);
            GL.Vertex3(x, 0, x);
            GL.Color3(Color.Yellow);
            GL.Vertex3(0, x, x);
            GL.Vertex3(x, x, x);
            GL.Vertex3(0, x, 0);
            GL.Vertex3(x, x, 0);
            GL.Color3(Color.Red);
            GL.Vertex3(0, 0, 0);
            GL.Vertex3(x, 0, 0);
            GL.End();

            GL.Begin(PrimitiveType.Quads);
            GL.Vertex3(x, 0, 0); 
            GL.Vertex3(x, 0, x);
            GL.Color3(Color.Yellow);
            GL.Vertex3(x, x, x);
            GL.Vertex3(x, x, 0);

            GL.Vertex3(0, x, 0);
            GL.Vertex3(0, x, x);
            GL.Color3(Color.Red);
            GL.Vertex3(0, 0, x);
            GL.Vertex3(0, 0, 0);

            GL.End();
        }


        [STAThread]
        static void Main(string[] args)
        {

            /**Utilizarea cuvântului-cheie "using" va permite dealocarea memoriei o dată ce obiectul nu mai este
               în uz (vezi metoda "Dispose()").
               Metoda "Run()" specifică cerința noastră de a avea 30 de evenimente de tip UpdateFrame per secundă
               și un număr nelimitat de evenimente de tip randare 3D per secundă (maximul suportat de subsistemul
               grafic). Asta nu înseamnă că vor primi garantat respectivele valori!!!
               Ideal ar fi ca după fiecare UpdateFrame să avem si un RenderFrame astfel încât toate obiectele generate
               în scena 3D să fie actualizate fără pierderi (desincronizări între logica aplicației și imaginea randată
               în final pe ecran). */
            using (ImmediateMode example = new ImmediateMode())
            {
                example.Run(30.0, 0.0);
            }
        }
    }

}
