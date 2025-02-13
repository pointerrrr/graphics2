﻿using System;
using System.IO;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace Template
{
	public class OpenTKApp : GameWindow
	{
		static int screenID1, screenID2;
		static Game game;
		static bool terminated = false;

		protected override void OnLoad( EventArgs e )
		{
			// called upon app init
			GL.ClearColor( Color.Black );
			GL.Enable( EnableCap.Texture2D );
			GL.Disable( EnableCap.DepthTest );
			GL.Hint( HintTarget.PerspectiveCorrectionHint, HintMode.Nicest );
			ClientSize = new Size( 1024, 512 );
			game = new Game();
			// initialize screens (for raytracer and debug)
			game.screen1 = new Surface( Width / 2, Height );
			game.screen2 = new Surface(Width / 2, Height);
			Sprite.target = game.screen1;
			screenID1 = game.screen1.GenTexture();
			screenID2 = game.screen2.GenTexture();
			game.Init();
		}
		protected override void OnUnload( EventArgs e )
		{
			// called upon app close
			GL.DeleteTextures(1, ref screenID1);
			GL.DeleteTextures(1, ref screenID2);
			Environment.Exit( 0 ); // bypass wait for key on CTRL-F5
		}
		protected override void OnResize( EventArgs e )
		{
			// called upon window resize
			GL.Viewport(0, 0, Width, Height);
			GL.MatrixMode( MatrixMode.Projection );
			GL.LoadIdentity();
			GL.Ortho( -1.0, 1.0, -1.0, 1.0, 0.0, 4.0 );
		}
		protected override void OnUpdateFrame( FrameEventArgs e )
		{
			// called once per frame; app logic
			KeyboardState keyboard = OpenTK.Input.Keyboard.GetState();
			game.Controls(keyboard);
			if (keyboard[Key.Escape]) this.Exit();
		}
		protected override void OnRenderFrame( FrameEventArgs e )
		{
			// called once per frame; render
			game.Tick();
			if (terminated) 
			{
				Exit();
				return;
			}
			// convert game.screen1 to OpenGL texture
			GL.BindTexture( TextureTarget.Texture2D, screenID1 );
			// stream first screen to OpenGL
			GL.TexImage2D( TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, 
						   game.screen1.width, game.screen1.height, 0, 
						   OpenTK.Graphics.OpenGL.PixelFormat.Bgra, 
						   PixelType.UnsignedByte, game.screen1.pixels 
						 );
			// clear window contents
			GL.Clear( ClearBufferMask.ColorBufferBit );
			// setup camera
			GL.MatrixMode( MatrixMode.Modelview );
			GL.LoadIdentity();
			GL.MatrixMode( MatrixMode.Projection );
			GL.LoadIdentity();
			// draw the first screen
			GL.Begin( PrimitiveType.Quads );
			GL.TexCoord2( 0.0f, 1.0f ); GL.Vertex2( -1.0f, -1.0f );
			GL.TexCoord2( 1.0f, 1.0f ); GL.Vertex2(  0.0f, -1.0f );
			GL.TexCoord2( 1.0f, 0.0f ); GL.Vertex2(  0.0f,  1.0f );
			GL.TexCoord2( 0.0f, 0.0f ); GL.Vertex2( -1.0f,  1.0f );
			GL.End();
			// convert game.scree2 to OpenGL texture
			GL.BindTexture(TextureTarget.Texture2D, screenID2);
			// stream second screen to )penGL
			GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba,
						   game.screen2.width, game.screen2.height, 0,
						   OpenTK.Graphics.OpenGL.PixelFormat.Bgra,
						   PixelType.UnsignedByte, game.screen2.pixels
						 );
			// draw the second screen
			GL.Begin(PrimitiveType.Quads);
			GL.TexCoord2(0.0f, 1.0f); GL.Vertex2(0.0f, -1.0f);
			GL.TexCoord2(1.0f, 1.0f); GL.Vertex2(1.0f, -1.0f);
			GL.TexCoord2(1.0f, 0.0f); GL.Vertex2(1.0f, 1.0f);
			GL.TexCoord2(0.0f, 0.0f); GL.Vertex2(0.0f, 1.0f);
			GL.End();
			// tell OpenTK we're done rendering
			SwapBuffers();
		}
		public static void Main( string[] args ) 
		{ 
			// entry point
			using (OpenTKApp app = new OpenTKApp()) { app.Run( 30.0, 30.0 ); }
		}
	}
}