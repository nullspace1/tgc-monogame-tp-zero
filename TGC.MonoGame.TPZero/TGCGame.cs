﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TGC.MonoGame.TP.Content.Models;

namespace TGC.MonoGame.TP
{
    /// <summary>
    ///     Clase principal del juego.
    /// </summary>
    public class TGCGame : Game
    {
        public const string ContentFolder3D = "Models/";
        public const string ContentFolderEffects = "Effects/";
        public const string ContentFolderMusic = "Music/";
        public const string ContentFolderSounds = "Sounds/";
        public const string ContentFolderSpriteFonts = "SpriteFonts/";
        public const string ContentFolderTextures = "Textures/";

        private GraphicsDeviceManager Graphics { get; }
        private CityScene City { get; set; }
        private Model CarModel { get; set; }
        private Matrix CarWorld { get; set; }
        private FollowCamera FollowCamera { get; set; }

        private float wheelForce = 13000f;
        private float dragCoefficient = 5f;

        private float wheelAngle = 0;

        private float WHEEL_ROTATION_SPEED = MathHelper.ToRadians(1f);

        private float MAX_WHEEL_ANGLE = MathHelper.ToRadians(30f);

        private float carRotationSpeed = 0.025f;

        private float tireSlipCoefficient = 2f;

        private float carMass = 20f;

        private Vector3 carVelocity = Vector3.Zero;



        /// <summary>
        ///     Constructor del juego.
        /// </summary>
        public TGCGame()
        {
            // Se encarga de la configuracion y administracion del Graphics Device.
            Graphics = new GraphicsDeviceManager(this);

            // Carpeta donde estan los recursos que vamos a usar.
            Content.RootDirectory = "Content";

            // Hace que el mouse sea visible.
            IsMouseVisible = true;
        }

        /// <summary>
        ///     Llamada una vez en la inicializacion de la aplicacion.
        ///     Escribir aca todo el codigo de inicializacion: Todo lo que debe estar precalculado para la aplicacion.
        /// </summary>
        protected override void Initialize()
        {
            // Enciendo Back-Face culling.
            // Configuro Blend State a Opaco.
            var rasterizerState = new RasterizerState();
            rasterizerState.CullMode = CullMode.CullCounterClockwiseFace;
            GraphicsDevice.RasterizerState = rasterizerState;
            GraphicsDevice.BlendState = BlendState.Opaque;

            // Configuro las dimensiones de la pantalla.
            Graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width - 100;
            Graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height - 100;
            Graphics.ApplyChanges();

            // Creo una camaar para seguir a nuestro auto.
            FollowCamera = new FollowCamera(GraphicsDevice.Viewport.AspectRatio);

            // Configuro la matriz de mundo del auto.
            CarWorld = Matrix.Identity;

            base.Initialize();
        }

        /// <summary>
        ///     Llamada una sola vez durante la inicializacion de la aplicacion, luego de Initialize, y una vez que fue configurado GraphicsDevice.
        ///     Debe ser usada para cargar los recursos y otros elementos del contenido.
        /// </summary>
        protected override void LoadContent()
        {
            // Creo la escena de la ciudad.
            City = new CityScene(Content);

            CarModel = Content.Load<Model>("Models/scene/car");

            base.LoadContent();
        }

        /// <summary>
        ///     Es llamada N veces por segundo. Generalmente 60 veces pero puede ser configurado.
        ///     La logica general debe ser escrita aca, junto al procesamiento de mouse/teclas.
        /// </summary>
        protected override void Update(GameTime gameTime)
        {


            float totalForce = 0;
            float dt = (float) gameTime.ElapsedGameTime.TotalSeconds;

            // Caputo el estado del teclado.
            var keyboardState = Keyboard.GetState();

            if (keyboardState.IsKeyDown(Keys.A))
            {
                wheelAngle = Math.Min(MAX_WHEEL_ANGLE, wheelAngle + WHEEL_ROTATION_SPEED);
            }
            else if (keyboardState.IsKeyDown(Keys.D))
            {
                wheelAngle = Math.Max(-MAX_WHEEL_ANGLE, wheelAngle - WHEEL_ROTATION_SPEED);
            }
            else
            {
                wheelAngle -= wheelAngle * 0.5f;
            }

            if (keyboardState.IsKeyDown(Keys.W))
            {
              totalForce = wheelForce;
            }

            if (keyboardState.IsKeyDown(Keys.S))
            {
              totalForce = - wheelForce;
            }

            Vector3 forwardAcceleration = CarWorld.Forward * totalForce / carMass;
            Vector3 dragAcceleration = -dragCoefficient * carVelocity / carMass;
            Vector3 tireSlipAcceleration = carVelocity.Length() < float.Epsilon ? Vector3.Zero : -tireSlipCoefficient * Vector3.Cross(CarWorld.Forward, carVelocity).Length() / carVelocity.Length() * carVelocity;

            carVelocity += (forwardAcceleration + dragAcceleration+ tireSlipAcceleration) * dt;

            

            CarWorld = Matrix.CreateFromAxisAngle(CarWorld.Up, wheelAngle * carVelocity.Length() * carRotationSpeed  * dt) * CarWorld;
            CarWorld *= Matrix.CreateTranslation(carVelocity * dt);


            if (keyboardState.IsKeyDown(Keys.Escape))
            {
                Exit();
            }

            // Actualizo la camara, enviandole la matriz de mundo del auto.
            FollowCamera.Update(gameTime, CarWorld);


            base.Update(gameTime);
        }


        /// <summary>
        ///     Llamada para cada frame.
        ///     La logica de dibujo debe ir aca.
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
            // Limpio la pantalla.
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // Dibujo la ciudad.
            City.Draw(gameTime, FollowCamera.View, FollowCamera.Projection);

            // El dibujo del auto debe ir aca.
            CarModel.Draw(CarWorld, FollowCamera.View, FollowCamera.Projection);


            base.Draw(gameTime);
        }

        /// <summary>
        ///     Libero los recursos cargados.
        /// </summary>
        protected override void UnloadContent()
        {
            // Libero los recursos cargados dessde Content Manager.
            Content.Unload();

            base.UnloadContent();
        }
    }
}