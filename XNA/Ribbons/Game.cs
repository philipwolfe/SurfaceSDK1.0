using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using Microsoft.Surface;
using Microsoft.Surface.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Ribbons
{
	public class Game : Microsoft.Xna.Framework.Game
	{
		public enum RibbonType
		{
			ERibbonNone,
			ERibbonSplat,
			ERibbonPoly
		}

		private readonly GraphicsDeviceManager graphics;

		private readonly ContentManager content;

		private ContactTarget contactTarget;

		private UserOrientation currentOrientation = UserOrientation.Bottom;

		private Color backgroundColor = Color.Black;

		private bool applicationLoadCompleteSignalled;

		private Matrix screenTransform = Matrix.Identity;

		private KeyboardState oldState;

		private bool activated = true;

		private BasicEffect basicColorEffect;

		private BasicEffect basicTextureEffect;

		private SpriteBatch spriteBatch;

		private Effect quadEffect;

		private VertexDeclaration colorVertexDecl;

		private VertexDeclaration colorTextureVertexDecl;

		private VertexDeclaration normalVertexDecl;

		private bool wireframeMode;

		private bool multisampleMode = true;

		private Matrix world;

		private Matrix[] worldEdges = new Matrix[8];

		private bool drawAllWorldEdges = true;

		private Matrix view;

		private Matrix projection;

		private bool drawBackground = true;

		private int backgroundTextureIdx = 1;

		private Texture2D logoBackground;

		private float logoAlpha = 1f;

		private Texture2D[] backTextures = new Texture2D[3];

		private Texture2D lensTexture;

		private Texture2D dialogBackTexture;

		private Texture2D[] buttonColorTextures = new Texture2D[4];

		private Texture2D buttonTypePolyTexture;

		private Texture2D buttonTypeSplatTexture;

		private Texture2D buttonClearTexture;

		public Texture2D SprayTexture;

		private PostProcessingGlow postGlow;

		private float blurFactor = 20f;

		private float shadowBlurFactor = 1f;

		private float glowFactor = 2f;

		private bool glowOn;

		private bool fovOn;

		private Effect polyEffect;

		private Effect splatEffect;

		private Effect depthEffect;

		private Effect blurEffect;

		private Effect blurShadowEffect;

		private Effect combineEffect;

		private RenderTarget2D fovDepthTarget;

		private RenderTarget2D blurTarget;

		private RenderTarget2D clearTarget;

		private bool shadowOn;

		private bool splineSmooth = true;

		private RibbonType ribbonType;

		private int ribbonIdx;

		private Dictionary<int, RibbonBase> ribbonMap = new Dictionary<int, RibbonBase>();

		private ArrayList ribbonArray = new ArrayList();

		private bool paused;

		private static Game instance;

		private int currentColorSet;

		private bool renderBlobDetect;

		private BlobDetectMgr blobDetectMgr;

		private byte[] currentImage;

		private ImageMetrics imageMetrics;

		private bool imageUpdated;

		public static int MetricsWidth;

		public static int MetricsHeight;

		private ArrayList logoBlobs = new ArrayList();

		private RibbonBase logoRibbon;

		private Config config = new Config();

		private Dictionary<int, int> buttonMap = new Dictionary<int, int>();

		private Vector2 buttonSize = new Vector2(48f, 50f);

		private Vector2[] buttonPositions = new Vector2[3]
		{
			new Vector2(970f, 335f),
			new Vector2(970f, 382f),
			new Vector2(970f, 428f)
		};

		private ReactLayer reactLayer = new ReactLayer();

		private ArrayList buttonTouches = new ArrayList();

		private static int imageUpdateCount;

		public Matrix World
		{
			get
			{
				return world;
			}
		}

		public Matrix[] WorldEdges
		{
			get
			{
				return worldEdges;
			}
		}

		public bool DrawAllWorldEdges
		{
			get
			{
				return drawAllWorldEdges;
			}
		}

		public Matrix View
		{
			get
			{
				return view;
			}
		}

		public Matrix Projection
		{
			get
			{
				return projection;
			}
		}

		public bool SplineSmooth
		{
			get
			{
				return splineSmooth;
			}
		}

		public static Game Instance
		{
			get
			{
				return instance;
			}
		}

		public BlobDetectMgr BlobMgr
		{
			get
			{
				return blobDetectMgr;
			}
		}

		public Config Config
		{
			get
			{
				return config;
			}
		}

		protected GraphicsDeviceManager Graphics
		{
			get
			{
				return graphics;
			}
		}

		public ContentManager ContentManager
		{
			get
			{
				return content;
			}
		}

		public ContactTarget ContactTarget
		{
			get
			{
				return contactTarget;
			}
		}

		public Game()
		{
			graphics = new GraphicsDeviceManager(this);
			content = new ContentManager(base.Services);
			if (File.Exists("config.xml"))
			{
				XmlSerializer xmlSerializer = new XmlSerializer(typeof(Config));
				FileStream fileStream = new FileStream("config.xml", FileMode.Open);
				if (fileStream.CanRead)
				{
					config = (Config)xmlSerializer.Deserialize(fileStream);
				}
			}
			Util.Init(DateTime.Now.Second);
			if (multisampleMode)
			{
				graphics.PreferMultiSampling = true;
				graphics.PreparingDeviceSettings += graphics_PreparingDeviceSettings;
			}
			float num = Util.Rand();
			if (config.RibbonStartType != 0)
			{
				SetRibbonType(config.RibbonStartType);
			}
			else
			{
				SetRibbonType((num > 0.5f) ? RibbonType.ERibbonSplat : RibbonType.ERibbonPoly);
			}
			instance = this;
			base.IsFixedTimeStep = true;
			base.TargetElapsedTime = TimeSpan.FromMilliseconds(33.333333333333336);
		}

		protected override void Initialize()
		{
			SetWindowOnSurface();
			InitializeSurfaceInput();
			currentOrientation = ApplicationLauncher.Orientation;
			if (currentOrientation == UserOrientation.Top)
			{
				Matrix matrix = Matrix.CreateRotationZ(MathHelper.ToRadians(180f));
				Matrix matrix2 = Matrix.CreateTranslation(graphics.GraphicsDevice.Viewport.Width, graphics.GraphicsDevice.Viewport.Height, 0f);
				screenTransform = matrix * matrix2;
			}
			postGlow = new PostProcessingGlow(graphics.GraphicsDevice);
			postGlow.Initialize();
			fovDepthTarget = new RenderTarget2D(graphics.GraphicsDevice, graphics.GraphicsDevice.Viewport.Width, graphics.GraphicsDevice.Viewport.Height, 1, SurfaceFormat.Color, MultiSampleType.NonMaskable, 1);
			blurTarget = new RenderTarget2D(graphics.GraphicsDevice, graphics.GraphicsDevice.Viewport.Width, graphics.GraphicsDevice.Viewport.Height, 1, SurfaceFormat.Color, MultiSampleType.NonMaskable, 1);
			clearTarget = new RenderTarget2D(graphics.GraphicsDevice, graphics.GraphicsDevice.Viewport.Width, graphics.GraphicsDevice.Viewport.Height, 1, SurfaceFormat.Color, MultiSampleType.NonMaskable, 1);
			if (config.ShowFramerate)
			{
				base.Components.Add(new FrameRateCounter(this));
			}
			base.Initialize();
		}

		private void SetWindowOnSurface()
		{
			if (!(base.Window.Handle == IntPtr.Zero))
			{
				InteractiveSurface defaultInteractiveSurface = InteractiveSurface.DefaultInteractiveSurface;
				if (defaultInteractiveSurface != null)
				{
					graphics.PreferredBackBufferWidth = defaultInteractiveSurface.Width;
					graphics.PreferredBackBufferHeight = defaultInteractiveSurface.Height;
					graphics.ApplyChanges();
					Program.RemoveBorder(base.Window.Handle);
					Program.PositionWindow(base.Window.Handle, defaultInteractiveSurface.Left, defaultInteractiveSurface.Top);
				}
			}
		}

		private void InitializeSurfaceInput()
		{
			if (!(base.Window.Handle == IntPtr.Zero) && contactTarget == null)
			{
				contactTarget = new ContactTarget(base.Window.Handle, EventThreadChoice.OnCurrentThread);
				contactTarget.EnableInput();
				contactTarget.EnableImage(ImageType.Normalized);
				contactTarget.FrameReceived += FrameReceived;
				ApplicationLauncher.ApplicationActivated += AppActivated;
				ApplicationLauncher.ApplicationDeactivated += AppDeactivated;
				ApplicationLauncher.ApplicationPreviewed += AppPreviewed;
			}
		}

		public void AppActivated(object sender, EventArgs e)
		{
			activated = true;
			contactTarget.EnableImage(ImageType.Normalized);
			contactTarget.FrameReceived += FrameReceived;
		}

		public void AppDeactivated(object sender, EventArgs e)
		{
			if (activated)
			{
				activated = false;
				contactTarget.DisableImage(ImageType.Normalized);
				contactTarget.FrameReceived -= FrameReceived;
			}
		}

		public void AppPreviewed(object sender, EventArgs e)
		{
			if (activated)
			{
				activated = false;
				contactTarget.DisableImage(ImageType.Normalized);
				contactTarget.FrameReceived -= FrameReceived;
			}
		}

		public void FrameReceived(object sender, FrameReceivedEventArgs args)
		{
			if (!activated)
			{
				return;
			}
			if (imageUpdateCount == 0)
			{
				if (currentImage == null)
				{
					int paddingLeft;
					int paddingRight;
					if (args.TryGetRawImage(ImageType.Normalized, 0, 0, InteractiveSurface.DefaultInteractiveSurface.Width, InteractiveSurface.DefaultInteractiveSurface.Height, out currentImage, out imageMetrics, out paddingLeft, out paddingRight))
					{
						imageUpdated = true;
					}
					if (imageMetrics != null)
					{
						MetricsWidth = imageMetrics.Width;
						MetricsHeight = imageMetrics.Height;
					}
				}
				else if (currentImage != null && args.UpdateRawImage(ImageType.Normalized, currentImage, 0, 0, InteractiveSurface.DefaultInteractiveSurface.Width, InteractiveSurface.DefaultInteractiveSurface.Height))
				{
					imageUpdated = true;
				}
			}
			imageUpdateCount = (imageUpdateCount + 1) % 10;
		}

		protected override void LoadContent()
		{
			world = Matrix.Identity;
			worldEdges[0] = world;
			WorldEdges[0].Translation = new Vector3(-1024f, 0f, 0f);
			worldEdges[1] = world;
			WorldEdges[1].Translation = new Vector3(1024f, 0f, 0f);
			worldEdges[2] = world;
			WorldEdges[2].Translation = new Vector3(0f, 768f, 0f);
			worldEdges[3] = world;
			WorldEdges[3].Translation = new Vector3(0f, -768f, 0f);
			worldEdges[4] = world;
			WorldEdges[4].Translation = new Vector3(-1024f, -768f, 0f);
			worldEdges[5] = world;
			WorldEdges[5].Translation = new Vector3(-1024f, 768f, 0f);
			worldEdges[6] = world;
			WorldEdges[6].Translation = new Vector3(1024f, -768f, 0f);
			worldEdges[7] = world;
			WorldEdges[7].Translation = new Vector3(1024f, 768f, 0f);
			view = Matrix.CreateLookAt(new Vector3(0f, 0f, 20f), Vector3.Zero, Vector3.UnitY);
			projection = Matrix.CreateOrthographicOffCenter(0f, 1024f, 768f, 0f, -200f, 200f);
			postGlow.LoadContent(ContentManager);
			logoBackground = content.Load<Texture2D>("Content/logoBackground");
			backTextures[0] = content.Load<Texture2D>("Content/back4");
			backTextures[1] = content.Load<Texture2D>("Content/brown_now");
			backTextures[2] = content.Load<Texture2D>("Content/back1");
			lensTexture = content.Load<Texture2D>("Content/lens");
			SprayTexture = content.Load<Texture2D>("Content/spray");
			dialogBackTexture = content.Load<Texture2D>("Content/UITextures/dialogBack");
			buttonColorTextures[0] = content.Load<Texture2D>("Content/UITextures/buttonColor1");
			buttonColorTextures[1] = content.Load<Texture2D>("Content/UITextures/buttonColor2");
			buttonColorTextures[2] = content.Load<Texture2D>("Content/UITextures/buttonColor3");
			buttonColorTextures[3] = content.Load<Texture2D>("Content/UITextures/buttonColor4");
			buttonTypePolyTexture = content.Load<Texture2D>("Content/UITextures/buttonTypePoly");
			buttonTypeSplatTexture = content.Load<Texture2D>("Content/UITextures/buttonTypeSplat");
			buttonClearTexture = content.Load<Texture2D>("Content/UITextures/buttonClear");
			basicColorEffect = new BasicEffect(graphics.GraphicsDevice, null);
			basicColorEffect.World = world;
			basicColorEffect.View = view;
			basicColorEffect.Projection = projection;
			basicColorEffect.TextureEnabled = false;
			basicColorEffect.VertexColorEnabled = true;
			basicTextureEffect = new BasicEffect(graphics.GraphicsDevice, null);
			basicTextureEffect.World = world;
			basicTextureEffect.View = view;
			basicTextureEffect.Projection = projection;
			basicTextureEffect.TextureEnabled = true;
			basicTextureEffect.VertexColorEnabled = true;
			polyEffect = content.Load<Effect>("Content/Effects/RibbonRender");
			polyEffect.Parameters["World"].SetValue(world);
			polyEffect.Parameters["View"].SetValue(view);
			polyEffect.Parameters["Projection"].SetValue(projection);
			Vector4[] array = new Vector4[3]
			{
				new Vector4(-0.526f, -0.526f, 0f, 1f),
				new Vector4(0.719f, 0.342f, 0.604f, 1f),
				new Vector4(0.454f, 0.766f, 0.454f, 1f)
			};
			Vector4[] value = new Vector4[3]
			{
				new Vector4(1.3f, 1.26f, 1.1f, 1E+07f),
				new Vector4(1.964f, 1.761f, 1.41f, 1E+07f),
				new Vector4(1.323f, 1.361f, 1.393f, 1E+07f)
			};
			polyEffect.Parameters["vecLightDir"].SetValue(array);
			polyEffect.Parameters["LightColor"].SetValue(value);
			polyEffect.Parameters["NumLights"].SetValue(array.Length);
			splatEffect = content.Load<Effect>("Content/Effects/SplatRender");
			splatEffect.Parameters["World"].SetValue(world);
			splatEffect.Parameters["View"].SetValue(view);
			splatEffect.Parameters["Projection"].SetValue(projection);
			depthEffect = content.Load<Effect>("Content/Effects/DepthRender");
			depthEffect.Parameters["World"].SetValue(world);
			depthEffect.Parameters["View"].SetValue(view);
			depthEffect.Parameters["Projection"].SetValue(projection);
			blurEffect = content.Load<Effect>("Content/Effects/GaussianBlur");
			blurShadowEffect = content.Load<Effect>("Content/Effects/ShadowBlur");
			combineEffect = content.Load<Effect>("Content/Effects/FOVCombine");
			quadEffect = content.Load<Effect>("Content/Effects/QuadRender");
			spriteBatch = new SpriteBatch(graphics.GraphicsDevice);
			normalVertexDecl = new VertexDeclaration(graphics.GraphicsDevice, VertexPositionColoredNormal.VertexElements);
			colorVertexDecl = new VertexDeclaration(graphics.GraphicsDevice, VertexPositionColor.VertexElements);
			colorTextureVertexDecl = new VertexDeclaration(graphics.GraphicsDevice, VertexPositionColorTexture.VertexElements);
			reactLayer.Init(graphics.GraphicsDevice);
		}

		protected override void UnloadContent()
		{
			content.Unload();
		}

		protected override void Update(GameTime gameTime)
		{
			if (!activated)
			{
				return;
			}
			UpdateBlobs(gameTime);
			UpdateRibbonContacts();
			UpdateKeyboard();
			reactLayer.Update(gameTime);
			ArrayList arrayList = new ArrayList();
			foreach (ButtonTouch buttonTouch in buttonTouches)
			{
				if (!buttonTouch.Update(gameTime))
				{
					arrayList.Add(buttonTouch);
				}
			}
			foreach (ButtonTouch item in arrayList)
			{
				buttonTouches.Remove(item);
			}
			if (gameTime.TotalGameTime.TotalSeconds > 5.0)
			{
				logoAlpha -= 0.005f;
			}
			if (logoAlpha < 0f && logoRibbon != null && logoRibbon.Attached)
			{
				logoRibbon.DetachBlob();
			}
			base.Update(gameTime);
		}

		private void ClearRibbons()
		{
			ribbonIdx = 0;
			foreach (RibbonBase item in ribbonArray)
			{
				item.Clear();
			}
			ribbonArray.Clear();
			ribbonMap.Clear();
		}

		private void SetRibbonType(RibbonType _ribbonType)
		{
			ribbonType = _ribbonType;
			if (ribbonType == RibbonType.ERibbonNone)
			{
				ribbonType = RibbonType.ERibbonSplat;
			}
			if (ribbonType == RibbonType.ERibbonPoly)
			{
				drawBackground = true;
				shadowOn = true;
				backgroundTextureIdx = 0;
			}
			else
			{
				drawBackground = true;
				shadowOn = false;
				backgroundTextureIdx = 1;
			}
			ClearRibbons();
		}

		private void UpdateKeyboard()
		{
			KeyboardState state = Keyboard.GetState();
			if (state.IsKeyDown(Keys.W) && !oldState.IsKeyDown(Keys.W))
			{
				drawAllWorldEdges = !drawAllWorldEdges;
			}
			if (state.IsKeyDown(Keys.M) && !oldState.IsKeyDown(Keys.M))
			{
				multisampleMode = !multisampleMode;
			}
			if (state.IsKeyDown(Keys.G) && !oldState.IsKeyDown(Keys.G))
			{
				glowOn = !glowOn;
			}
			if (state.IsKeyDown(Keys.D) && !oldState.IsKeyDown(Keys.D))
			{
				renderBlobDetect = !renderBlobDetect;
			}
			if (state.IsKeyDown(Keys.P) && !oldState.IsKeyDown(Keys.P))
			{
				paused = !paused;
			}
			if (state.IsKeyDown(Keys.B) && !oldState.IsKeyDown(Keys.B))
			{
				drawBackground = !drawBackground;
			}
			if (state.IsKeyDown(Keys.S) && !oldState.IsKeyDown(Keys.S))
			{
				shadowOn = !shadowOn;
			}
			if (state.IsKeyDown(Keys.M) && !oldState.IsKeyDown(Keys.M))
			{
				splineSmooth = !splineSmooth;
			}
			if (state.IsKeyDown(Keys.Space) && !oldState.IsKeyDown(Keys.Space))
			{
				ClearRibbons();
			}
			if (state.IsKeyDown(Keys.Q) && !oldState.IsKeyDown(Keys.Q))
			{
				Exit();
			}
			if (state.IsKeyDown(Keys.OemPlus) && !oldState.IsKeyDown(Keys.OemPlus))
			{
				glowFactor += 1f;
			}
			if (state.IsKeyDown(Keys.OemMinus) && !oldState.IsKeyDown(Keys.OemMinus))
			{
				glowFactor -= 1f;
			}
			if (state.IsKeyDown(Keys.R) && !oldState.IsKeyDown(Keys.R))
			{
				ClearRibbons();
				SetRibbonType((RibbonType)((int)(ribbonType + 1) % 3));
			}
			if (state.IsKeyDown(Keys.C) && !oldState.IsKeyDown(Keys.C))
			{
				ClearRibbons();
			}
			if (state.IsKeyDown(Keys.T) && !oldState.IsKeyDown(Keys.T))
			{
				backgroundTextureIdx = (backgroundTextureIdx + 1) % backTextures.Length;
			}
			oldState = state;
		}

		public ButtonTouch AddButtonTouch(Vector2 touchPt)
		{
			ButtonTouch buttonTouch = new ButtonTouch(touchPt);
			buttonTouches.Add(buttonTouch);
			return buttonTouch;
		}

		private void UpdateRibbonContacts()
		{
			ReadOnlyContactCollection state = contactTarget.GetState();
			ArrayList arrayList = new ArrayList();
			arrayList.AddRange(ribbonMap.Keys);
			arrayList.AddRange(buttonMap.Keys);
			foreach (Contact item in state)
			{
				RibbonBase value;
				int value2;
				if (ribbonMap.TryGetValue(item.Id, out value))
				{
					arrayList.Remove(item.Id);
					if (value != null)
					{
						float num = value.DistToLast(item.X, item.Y);
						if (num > 0f)
						{
							value.AddPoint(item.X, item.Y, CalcContactArea(value, item));
							value.Smooth();
							value.BuildPolys();
						}
					}
				}
				else if (buttonMap.TryGetValue(item.Id, out value2))
				{
					arrayList.Remove(item.Id);
				}
				else
				{
					if (!item.IsFingerRecognized)
					{
						continue;
					}
					Vector2 point = new Vector2(item.X, item.Y);
					bool flag = false;
					if (BlobMgr != null)
					{
						foreach (Blob blob in BlobMgr.GetBlobs())
						{
							if (blob.width * blob.height > 40f && blob.IsPointInside(point))
							{
								flag = true;
							}
						}
					}
					if (!flag)
					{
						RibbonBase ribbonBase = null;
						switch (ribbonType)
						{
						case RibbonType.ERibbonPoly:
							ribbonBase = new RibbonPoly();
							break;
						case RibbonType.ERibbonSplat:
							ribbonBase = new RibbonSplat();
							break;
						}
						int num2 = (int)Math.Ceiling((double)item.Orientation / (Math.PI * 2.0) * 8.0);
						if (num2 == 0)
						{
							num2 = 1;
						}
						ribbonBase.Init(num2, 1024, 768);
						ribbonBase.Clear();
						ribbonBase.AddPoint(item.X, item.Y, CalcContactArea(ribbonBase, item));
						ribbonIdx++;
						ribbonArray.Insert(0, ribbonBase);
						if (ribbonArray.Count > config.MaxRibbons)
						{
							ribbonArray.RemoveAt(ribbonArray.Count - 1);
						}
						ribbonMap[item.Id] = ribbonBase;
					}
				}
			}
			foreach (int item2 in arrayList)
			{
				if (ribbonMap.ContainsKey(item2))
				{
					ribbonMap.Remove(item2);
				}
				if (buttonMap.ContainsKey(item2))
				{
					buttonMap.Remove(item2);
				}
			}
		}

		private float CalcContactArea(RibbonBase ribbon, Contact contact)
		{
			Vector3 lastPoint = ribbon.GetLastPoint();
			if (lastPoint == Vector3.Zero)
			{
				return contact.PhysicalArea;
			}
			Vector2 value = new Vector2(lastPoint.X - contact.X, lastPoint.Y - contact.Y);
			value.Normalize();
			float value2 = Vector2.Dot(value, new Vector2(contact.Bounds.Height, contact.Bounds.Width));
			return Math.Abs(value2);
		}

		private void UpdateRibbonGeometry()
		{
			ArrayList arrayList = new ArrayList();
			foreach (RibbonBase item in ribbonArray)
			{
				if (!ribbonMap.ContainsValue(item))
				{
					if (!item.IsValid())
					{
						arrayList.Add(item);
					}
					else
					{
						UpdateRibbon(item);
					}
				}
			}
			foreach (RibbonBase item2 in arrayList)
			{
				ribbonArray.Remove(item2);
			}
		}

		private void UpdateRibbon(RibbonBase ribbon)
		{
			if (ribbon.exists)
			{
				if (!ribbon.detached)
				{
					ribbon.DetachFromContact();
				}
				ribbon.Update();
			}
		}

		private void graphics_PreparingDeviceSettings(object sender, PreparingDeviceSettingsEventArgs e)
		{
			int qualityLevels = 0;
			GraphicsAdapter adapter = e.GraphicsDeviceInformation.Adapter;
			SurfaceFormat format = adapter.CurrentDisplayMode.Format;
			if (adapter.CheckDeviceMultiSampleType(DeviceType.Hardware, format, false, MultiSampleType.FourSamples, out qualityLevels))
			{
				e.GraphicsDeviceInformation.PresentationParameters.MultiSampleQuality = 1;
				e.GraphicsDeviceInformation.PresentationParameters.MultiSampleType = MultiSampleType.NonMaskable;
			}
			else if (adapter.CheckDeviceMultiSampleType(DeviceType.Hardware, format, false, MultiSampleType.TwoSamples, out qualityLevels))
			{
				e.GraphicsDeviceInformation.PresentationParameters.MultiSampleQuality = 1;
				e.GraphicsDeviceInformation.PresentationParameters.MultiSampleType = MultiSampleType.NonMaskable;
			}
		}

		protected override void Draw(GameTime gameTime)
		{
			if (!applicationLoadCompleteSignalled)
			{
				ApplicationLauncher.SignalApplicationLoadComplete();
				applicationLoadCompleteSignalled = true;
			}
			if (activated)
			{
				graphics.GraphicsDevice.VertexDeclaration = normalVertexDecl;
				graphics.GraphicsDevice.RenderState.CullMode = CullMode.None;
				graphics.GraphicsDevice.RenderState.FillMode = (wireframeMode ? FillMode.WireFrame : FillMode.Solid);
				graphics.GraphicsDevice.RenderState.MultiSampleAntiAlias = multisampleMode;
				graphics.GraphicsDevice.RenderState.AlphaBlendEnable = true;
				graphics.GraphicsDevice.RenderState.DepthBufferEnable = false;
				graphics.GraphicsDevice.RenderState.SourceBlend = Blend.SourceAlpha;
				graphics.GraphicsDevice.RenderState.DestinationBlend = Blend.InverseSourceAlpha;
				graphics.GraphicsDevice.RenderState.BlendFunction = BlendFunction.Add;
				UpdateRibbonGeometry();
				if (fovOn)
				{
					FOVDrawRibbons(graphics.GraphicsDevice);
				}
				else if (shadowOn)
				{
					ShadowDrawRibbons(graphics.GraphicsDevice);
				}
				else
				{
					DrawRibbons(graphics.GraphicsDevice);
				}
				if (glowOn)
				{
					postGlow.Draw(glowFactor);
				}
				if (renderBlobDetect)
				{
					RenderBlobs(graphics.GraphicsDevice);
				}
				reactLayer.Draw(graphics.GraphicsDevice);
				base.Draw(gameTime);
			}
		}

		private void DrawDialog(GraphicsDevice device)
		{
			DrawQuad(dialogBackTexture, quadEffect);
			DrawQuad(buttonColorTextures[currentColorSet], quadEffect, buttonPositions[0], 1f);
			if (ribbonType == RibbonType.ERibbonPoly)
			{
				DrawQuad(buttonTypeSplatTexture, quadEffect, buttonPositions[1], 1f);
			}
			else
			{
				DrawQuad(buttonTypePolyTexture, quadEffect, buttonPositions[1], 1f);
			}
			DrawQuad(buttonClearTexture, quadEffect, buttonPositions[2], 1f);
		}

		private void UpdateBlobs(GameTime gameTime)
		{
			if (currentImage != null && imageUpdated)
			{
				if (blobDetectMgr == null)
				{
					blobDetectMgr = new BlobDetectMgr(MetricsWidth, MetricsHeight);
					blobDetectMgr.SetThreshold(0.38f);
				}
				blobDetectMgr.UpdateBlobs(gameTime, currentImage);
				GC.Collect();
			}
			if (blobDetectMgr != null)
			{
				blobDetectMgr.UpdateBlobsMarkForRemove(gameTime);
			}
			imageUpdated = false;
		}

		private void RenderBlobs(GraphicsDevice device)
		{
			basicColorEffect.Begin();
			foreach (EffectPass pass in basicColorEffect.CurrentTechnique.Passes)
			{
				pass.Begin();
				if (Instance.BlobMgr != null)
				{
					foreach (Blob blob3 in Instance.BlobMgr.GetBlobs())
					{
						blob3.Render(device);
					}
				}
				foreach (Blob logoBlob in logoBlobs)
				{
					logoBlob.Render(device);
				}
				pass.End();
			}
			basicColorEffect.End();
		}

		private void FOVDrawRibbons(GraphicsDevice device)
		{
			DepthStencilBuffer depthStencilBuffer = device.DepthStencilBuffer;
			device.DepthStencilBuffer = null;
			device.SetRenderTarget(0, fovDepthTarget);
			device.Clear(ClearOptions.Target, Color.TransparentBlack, 1f, 1);
			switch (ribbonType)
			{
			case RibbonType.ERibbonPoly:
				DrawPolyRibbons(device, depthEffect);
				break;
			case RibbonType.ERibbonSplat:
				DrawSplatRibbons(device, depthEffect);
				break;
			}
			device.SetRenderTarget(0, clearTarget);
			device.Clear(ClearOptions.Target, Color.TransparentBlack, 1f, 1);
			switch (ribbonType)
			{
			case RibbonType.ERibbonPoly:
				DrawPolyRibbons(device, polyEffect);
				break;
			case RibbonType.ERibbonSplat:
				DrawSplatRibbons(device, splatEffect);
				break;
			}
			device.SetRenderTarget(0, blurTarget);
			GaussianHelper.SetBlurEffectParameters(ref blurEffect, blurFactor / (float)device.Viewport.Width, blurFactor / (float)device.Viewport.Height);
			device.Clear(ClearOptions.Target, Color.TransparentBlack, 1f, 1);
			Texture2D texture = clearTarget.GetTexture();
			DrawQuad(texture, blurEffect);
			device.DepthStencilBuffer = depthStencilBuffer;
			device.SetRenderTarget(0, null);
			device.Clear(ClearOptions.Target, config.GetBackgroundColor(), 1f, 1);
			device.Textures[1] = blurTarget.GetTexture();
			device.Textures[2] = fovDepthTarget.GetTexture();
			DrawQuad(clearTarget.GetTexture(), combineEffect);
		}

		private void ShadowDrawRibbons(GraphicsDevice device)
		{
			DepthStencilBuffer depthStencilBuffer = device.DepthStencilBuffer;
			device.DepthStencilBuffer = null;
			device.SetRenderTarget(0, clearTarget);
			device.Clear(ClearOptions.Target, Color.TransparentBlack, 1f, 1);
			switch (ribbonType)
			{
			case RibbonType.ERibbonPoly:
				DrawPolyRibbons(device, polyEffect);
				break;
			case RibbonType.ERibbonSplat:
				DrawSplatRibbons(device, splatEffect);
				break;
			}
			device.SetRenderTarget(0, blurTarget);
			GaussianHelper.SetBlurEffectParameters(ref blurShadowEffect, shadowBlurFactor / (float)device.Viewport.Width, shadowBlurFactor / (float)device.Viewport.Height);
			device.Clear(ClearOptions.Target, Color.TransparentBlack, 1f, 1);
			blurShadowEffect.Parameters["ShadowColor"].SetValue(Color.Black.ToVector4());
			DrawQuad(clearTarget.GetTexture(), blurShadowEffect);
			device.SetRenderTarget(0, null);
			device.Clear(ClearOptions.Target, Color.Black, 1f, 1);
			if (drawBackground)
			{
				DrawQuad(backTextures[backgroundTextureIdx], quadEffect);
			}
			DrawQuad(blurTarget.GetTexture(), quadEffect);
			DrawQuad(clearTarget.GetTexture(), quadEffect);
			device.DepthStencilBuffer = depthStencilBuffer;
		}

		public Color GetColor(int color)
		{
			return config.GetColor(currentColorSet + 1, color);
		}

		private void DrawRibbons(GraphicsDevice device)
		{
			device.Clear(ClearOptions.Target, Color.TransparentBlack, 1f, 1);
			if (drawBackground)
			{
				DrawQuad(backTextures[backgroundTextureIdx], quadEffect);
			}
			switch (ribbonType)
			{
			case RibbonType.ERibbonPoly:
				DrawPolyRibbons(device, polyEffect);
				break;
			case RibbonType.ERibbonSplat:
				DrawSplatRibbons(device, splatEffect);
				break;
			}
		}

		private void DrawButtonTouches(GraphicsDevice device)
		{
			basicColorEffect.Begin();
			graphics.GraphicsDevice.VertexDeclaration = normalVertexDecl;
			foreach (EffectPass pass in basicColorEffect.CurrentTechnique.Passes)
			{
				pass.Begin();
				graphics.GraphicsDevice.RenderState.BlendFunction = BlendFunction.Add;
				graphics.GraphicsDevice.RenderState.SourceBlend = Blend.SourceAlpha;
				graphics.GraphicsDevice.RenderState.DestinationBlend = Blend.One;
				foreach (ButtonTouch buttonTouch in buttonTouches)
				{
					buttonTouch.Draw(graphics.GraphicsDevice);
				}
				pass.End();
			}
			basicColorEffect.End();
		}

		private void DrawQuad(Texture2D texture, Effect effect)
		{
			DrawQuad(texture, effect, Vector2.Zero, 1f);
		}

		private void DrawQuad(Texture2D texture, Effect effect, Vector2 pos, float alpha)
		{
			effect.Begin();
			spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.SaveState);
			foreach (EffectPass pass in effect.CurrentTechnique.Passes)
			{
				pass.Begin();
				spriteBatch.Draw(texture, pos, new Color(new Vector4(1f, 1f, 1f, alpha)));
				pass.End();
			}
			spriteBatch.End();
			effect.End();
		}

		public void DrawQuad(GraphicsDevice device, Effect effect, Vector2 ll, Vector2 ul, Vector2 lr, Vector2 ur, Color color)
		{
			effect.Begin();
			foreach (EffectPass pass in effect.CurrentTechnique.Passes)
			{
				pass.Begin();
				VertexPositionColorTexture[] array = new VertexPositionColorTexture[4];
				int[] array2 = new int[6];
				int num = 0;
				int num2 = 0;
				float z = 0f;
				array[num].Position = new Vector3(ul, z);
				array[num].TextureCoordinate = new Vector2(0f, 0f);
				array[num++].Color = color;
				array[num].Position = new Vector3(ll, z);
				array[num].TextureCoordinate = new Vector2(0f, 1f);
				array[num++].Color = color;
				array[num].Position = new Vector3(ur, z);
				array[num].TextureCoordinate = new Vector2(1f, 0f);
				array[num++].Color = color;
				array[num].Position = new Vector3(lr, z);
				array[num].TextureCoordinate = new Vector2(1f, 1f);
				array[num++].Color = color;
				array2[num2++] = 0;
				array2[num2++] = 1;
				array2[num2++] = 2;
				array2[num2++] = 2;
				array2[num2++] = 1;
				array2[num2++] = 3;
				device.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, array, 0, 4, array2, 0, 2);
				pass.End();
			}
			effect.End();
		}

		private void DrawPolyRibbons(GraphicsDevice device, Effect effect)
		{
			ArrayList arrayList = (ArrayList)ribbonArray.Clone();
			arrayList.Reverse();
			effect.Begin();
			foreach (EffectPass pass in effect.CurrentTechnique.Passes)
			{
				pass.Begin();
				foreach (RibbonPoly item in arrayList)
				{
					if (item.exists && item.IsValid())
					{
						item.Draw(device, effect);
					}
				}
				pass.End();
			}
			effect.End();
		}

		private void DrawSplatRibbons(GraphicsDevice device, Effect effect)
		{
			ArrayList arrayList = (ArrayList)ribbonArray.Clone();
			arrayList.Reverse();
			spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.SaveState);
			graphics.GraphicsDevice.RenderState.AlphaBlendEnable = true;
			graphics.GraphicsDevice.RenderState.SourceBlend = Blend.SourceAlpha;
			graphics.GraphicsDevice.RenderState.DestinationBlend = Blend.One;
			graphics.GraphicsDevice.RenderState.BlendFunction = BlendFunction.Add;
			effect.Begin();
			foreach (EffectPass pass in effect.CurrentTechnique.Passes)
			{
				pass.Begin();
				foreach (RibbonBase item in arrayList)
				{
					if (item.exists && item.IsValid())
					{
						item.Draw(device, effect, spriteBatch);
					}
				}
				pass.End();
			}
			effect.End();
			spriteBatch.End();
		}

		public float GetZVal(RibbonPoly ribbon)
		{
			int num = 0;
			foreach (RibbonPoly item in ribbonArray)
			{
				if (item != ribbon)
				{
					num++;
					continue;
				}
				break;
			}
			float num2 = (float)num / (float)config.MaxRibbons * 100f;
			return num2 + 1f;
		}

		public Vector3 TransformPt(Vector3 pt)
		{
			Vector3 vector = Vector3.Transform(pt, world);
			vector = Vector3.Transform(pt, view);
			return Vector3.Transform(pt, projection);
		}
	}
}
