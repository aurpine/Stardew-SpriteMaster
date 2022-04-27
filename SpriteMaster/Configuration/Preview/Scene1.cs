﻿using SpriteMaster.Types;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SpriteMaster.Configuration.Preview;

sealed class Scene1 : Scene {
	//private static readonly Lazy<Texture2D> FishTexture = new(() => StardewValley.Game1.content.Load<Texture2D>(@"Maps\springobjects"));
	private readonly AnimatedTexture CenterCharacterTexture;

	private readonly SpriteSheet OutdoorTiles;

	private readonly UniformAnimatedTexture OutdoorTilesWeed;

	private readonly UniformAnimatedTexture OutdoorTilesFlower;

	private readonly SpriteSheet TreeTexture;

	private DrawableInstance[] Drawables;

	private readonly string Season;

	protected override PrecipitationType Precipitation => ScenePrecipitation;
	private readonly PrecipitationType ScenePrecipitation;

	private static ref T GetRandomOf<T>(Random rand, T[] array) {
		if (array.Length == 0) {
			throw new ArgumentException("array is empty");
		}

		return ref array[rand.Next(array.Length)];
	}

	private const string KiwiPath = @"Characters\Kiwi";

	private static AnimatedTexture? GetCenterCharacterRSVKiwi() {
		try {
			return new UniformAnimatedTexture(
				textureName: KiwiPath,
				spriteSize: (16, 32),
				spriteOffset: (0, 4),
				spritesPerRow: 4,
				numSprites: 12,
				ticksPerFrame: 6
			);
		}
		catch {
			return null;
		}
	}

	private const string JunimoGoldenPath = @"Characters\JunimoGolden";

	private static AnimatedTexture? GetCenterCharacterESJunimoGolden() {
		try {
			return new UniformExplicitAnimatedTexture(
				textureName: JunimoGoldenPath,
				spriteSize: (16, 32),
				spriteOffset: (0, 4),
				spritesPerRow: 4,
				spriteIndices: new Vector2I[] {
					(1, 0),
					(0, 0),
					(3, 0),
					(2, 0),
					(1, 0)
				},
				ticksPerFrame: 6
			);
		}
		catch {
			return null;
		}
	}

	private const string JunimoPath = @"Characters\Junimo";

	private static AnimatedTexture? GetCenterCharacterJunimo() {
		try {
			return new UniformAnimatedTexture(
				textureName: JunimoPath,
				spriteSize: (16, 16),
				spriteOffset: (0, 0),
				spritesPerRow: 8,
				numSprites: 48,
				ticksPerFrame: 6
			);
		}
		catch {
			return null;
		}
	}

	private static AnimatedTexture? MakeCharacterTexture(string path) {
		try {
			return new UniformExplicitAnimatedTexture(
				textureName: path,
				spriteSize: (16, 32),
				spriteOffset: (0, 9),
				spritesPerRow: 4,
				spriteIndices: new Vector2I[] {
					//(0, 0),
					(0, 1),
					(1, 1),
					(0, 1),
					(2, 1),
					(0, 1),
					(3, 1),
					(3, 2),
					(2, 2),
					(3, 2),
					(3, 1),
					(0, 1)
				},
				ticksPerFrame: 24
			);
		}
		catch {
			return null;
		}
	}

	private static readonly string[] CharacterPaths = new[] {
		// vanilla
		@"Characters\Penny",
		@"Characters\Haley",
		@"Characters\Abigail",
		@"Characters\Maru",
		@"Characters\Leah",
		// sve
		@"Characters\Alesia",
		@"Characters\Claire",
		@"Characters\Olivia",
		@"Characters\Sophia",
		// rsv
		@"Characters\Alissa",
		@"Characters\Corine",
		@"Characters\Daia",
		@"Characters\Flor",
		@"Characters\Maddie",
		@"Characters\Ysabelle",
		// es
		@"Characters\Aideen",
		// rsv kiwi
		KiwiPath,
		// es junimo golden
		JunimoGoldenPath,
		// vanilla junimo
		JunimoPath
	};

	private static AnimatedTexture GetCenterCharacter() {
		var rand = new Random(Guid.NewGuid().GetHashCode());
		var characters = CharacterPaths.OrderBy(_ => rand.Next());

		foreach (var character in characters) {
			AnimatedTexture? result = character switch {
				KiwiPath => GetCenterCharacterRSVKiwi(),
				JunimoGoldenPath => GetCenterCharacterESJunimoGolden(),
				JunimoPath => GetCenterCharacterJunimo(),
				_ => MakeCharacterTexture(character)
			};
			if (result is null) {
				continue;
			}
			return result;
		}

		return MakeCharacterTexture(CharacterPaths[0])!;
	}

	private static readonly string[] Seasons = new[] {
		"spring",
		"summer",
		"fall",
		"winter"
	};

	internal Scene1(in Bounds scissor) : base(in scissor) {
		CenterCharacterTexture = GetCenterCharacter();

		var rand = new Random(Guid.NewGuid().GetHashCode());

		Season = Seasons[rand.Next(Seasons.Length)];

		if (Season == "winter") {
			ScenePrecipitation = PrecipitationType.Snow;
		}
		else {
			ScenePrecipitation = PrecipitationType.Rain;
		}

		string outdoorsTileSheet = $@"Maps\{Season}_outdoorsTileSheet";

		OutdoorTiles = new(
			textureName: outdoorsTileSheet,
			spriteSize: (16, 16)
		);

		if (Season == "winter") {
			OutdoorTilesWeed = new(
				textureName: outdoorsTileSheet,
				spriteSize: (16, 16),
				spriteOffset: (0, 12),
				spritesPerRow: 4,
				numSprites: 1,
				ticksPerFrame: 12
			);

			OutdoorTilesFlower = new(
				textureName: outdoorsTileSheet,
				spriteSize: (16, 16),
				spriteOffset: (0, 12),
				spritesPerRow: 4,
				numSprites: 1,
				ticksPerFrame: 12
			);
		}
		else {
			OutdoorTilesWeed = new(
				textureName: outdoorsTileSheet,
				spriteSize: (16, 16),
				spriteOffset: (20, 16),
				spritesPerRow: 4,
				numSprites: 4,
				ticksPerFrame: 12
			);

			OutdoorTilesFlower = new(
				textureName: outdoorsTileSheet,
				spriteSize: (16, 16),
				spriteOffset: (1, 6),
				spritesPerRow: 2,
				numSprites: 2,
				ticksPerFrame: 12
			);
		}

		TreeTexture = new(
			textureName: outdoorsTileSheet,
			spriteSize: (48, 96)
		);

		Drawables = SetupScene(Season == "winter");
	}

	public override void Dispose() {
		CenterCharacterTexture?.Dispose();
	}

	private const string ReferenceBasicText = "Llanfairpwllgwyngyllgogerychwyrndrobwllllantysiliogogogoch";
	private XNA.Graphics.SpriteFont BasicTextFont => StardewValley.Game1.dialogueFont;
	private const string ReferenceUtilityText = "It was the best of times, it was the blurst of times.";
	private XNA.Graphics.SpriteFont UtilityTextFont => StardewValley.Game1.smallFont;

	private static readonly Vector2I[] ShadowedStringOffsets = { (-1, -1), (1, -1), (-1, 1), (1, 1) };
	private void DrawStringShadowed(
		XNA.Graphics.SpriteBatch batch,
		XNA.Graphics.SpriteFont font,
		string text,
		Vector2I position,
		XNA.Color color
	) {
		var shadowColor = new XNA.Color(
			~color.R,
			~color.G,
			~color.B,
			color.A
		);

		foreach (var offset in ShadowedStringOffsets) {
			batch.DrawString(
				font,
				text,
				position + offset,
				shadowColor
			);
		}

		batch.DrawString(
			font,
			text,
			position,
			color
		);
	}

	protected override void OnDraw(XNA.Graphics.SpriteBatch batch, in Preview.Override overrideState) {
		for (int i = 0; i < Drawables.Length; ++i) {
			Drawables[i].Tick();
			Drawables[i].Draw(this, batch);
		}
	}

	protected override void OnDrawOverlay(XNA.Graphics.SpriteBatch batch, in Preview.Override overrideState) {
		{
			// Draw basic text
			DrawStringShadowed(
				batch,
				BasicTextFont,
				ReferenceBasicText,
				Region.Offset,
				XNA.Color.White
			);
		}

		{
			// Draw sprite text
			var textMeasure = UtilityTextFont.MeasureString(ReferenceUtilityText);

			var offset = Region.Offset + Region.Extent;
			offset -= (Vector2F)textMeasure;

			Utility.drawTextWithShadow(
				b: batch,
				text: ReferenceUtilityText,
				font: UtilityTextFont,
				position: offset,
				color: XNA.Color.White, //Game1.textColor,
				scale: 1.0f
			);
		}
	}

	private const int Seed = 0x13371337;

	private DrawableInstance[] SetupScene(bool winter) {
		var center = Size / 2;

		var rand = new Random(Seed);

		var plainGrass = winter ?
			new Drawable[] {
				OutdoorTiles[1, 14],
				OutdoorTiles[1, 14],
				OutdoorTiles[1, 14]
			} : new Drawable[] {
				OutdoorTiles[0, 7],
				OutdoorTiles[0, 11],
				OutdoorTiles[2, 16]
			};
		var grassArray = winter ?
			new Drawable[] {
				OutdoorTilesWeed,
				OutdoorTilesFlower,
				plainGrass[0],
				plainGrass[0],
				plainGrass[0],
				plainGrass[1],
				plainGrass[1],
				plainGrass[1],
				plainGrass[2],
				plainGrass[2],
				plainGrass[2],
				plainGrass[2],
				OutdoorTiles[4, 12],
				OutdoorTiles[5, 12],
				OutdoorTiles[4, 13],
			} :
			new Drawable[] {
				OutdoorTilesWeed,
				OutdoorTilesFlower,
				plainGrass[0],
				plainGrass[0],
				plainGrass[0],
				plainGrass[1],
				plainGrass[1],
				plainGrass[1],
				plainGrass[2],
				plainGrass[2],
				plainGrass[2],
				plainGrass[2],
				OutdoorTiles[0, 11],
				OutdoorTiles[4, 10],
				OutdoorTiles[5, 10],
			};
		var debrisArray = new Drawable[] {
			OutdoorTiles[7, 9],
			OutdoorTiles[7, 10],
			OutdoorTiles[7, 11],
			OutdoorTiles[7, 12],
		};
		int debrisChance = 20;

		var spriteSize = OutdoorTiles.RenderedSize;

		// We want the grass to be centered, rather than offset to the bounds
		var half = center;
		var alignedHalfCount = half / spriteSize;
		alignedHalfCount += (1, 1);
		var alignedHalf = alignedHalfCount * spriteSize;
		var start = center - alignedHalf;

		Vector2I tileCount = (alignedHalfCount * 2) + (1, 1);
		tileCount = tileCount.Max((6, 6));
		Vector2I mid = tileCount / 2;

		var tileArray = new List<Drawable>[tileCount.X, tileCount.Y];

		for (int y = 0; y < tileCount.Y; ++y) {
			for (int x = 0; x < tileCount.X; ++x) {
				var drawables = (tileArray[x, y] ??= new());

				var grassObject = GetRandomOf(rand, grassArray);
				drawables.Add(grassObject);

				if ((Math.Abs(x - mid.X) > 1 && Math.Abs(y - mid.Y) > 1) && rand.Next(100) < debrisChance) {
					var debrisObject = GetRandomOf(rand, debrisArray);
					drawables.Add(debrisObject);
				}
			}
		}

		if (winter) {
			tileArray[mid.X - 2, mid.Y - 2][0] = OutdoorTiles[3, 14];
			tileArray[mid.X - 1, mid.Y - 2][0] = OutdoorTiles[1, 15];
			tileArray[mid.X, mid.Y - 2][0] = OutdoorTiles[1, 15];
			tileArray[mid.X + 1, mid.Y - 2][0] = OutdoorTiles[1, 15];
			tileArray[mid.X + 2, mid.Y - 2][0] = OutdoorTiles[3, 16];
		}

		if (winter) tileArray[mid.X - 2, mid.Y - 1][0] = OutdoorTiles[2, 14];
		tileArray[mid.X - 1, mid.Y - 1][0] = OutdoorTiles[0, 8];
		tileArray[mid.X,		 mid.Y - 1][0] = OutdoorTiles[1, 8];
		tileArray[mid.X + 1, mid.Y - 1][0] = OutdoorTiles[3, 8];
		if (winter) tileArray[mid.X + 2, mid.Y - 1][0] = OutdoorTiles[0, 14];

		if (winter) tileArray[mid.X - 2, mid.Y		][0] = OutdoorTiles[2, 14];
		tileArray[mid.X - 1, mid.Y		][0] = OutdoorTiles[0, 9];
		tileArray[mid.X,		 mid.Y		][0] = OutdoorTiles[6, 8];
		tileArray[mid.X + 1, mid.Y		][0] = OutdoorTiles[3, 9];
		if (winter) tileArray[mid.X + 2, mid.Y		][0] = OutdoorTiles[0, 14];

		if (winter) tileArray[mid.X - 2, mid.Y + 1][0] = OutdoorTiles[2, 14];
		tileArray[mid.X - 1, mid.Y + 1][0] = OutdoorTiles[0, 10];
		tileArray[mid.X,		 mid.Y + 1][0] = OutdoorTiles[1, 10];
		tileArray[mid.X + 1, mid.Y + 1][0] = OutdoorTiles[3, 10];
		if (winter) tileArray[mid.X + 2, mid.Y + 1][0] = OutdoorTiles[0, 14];

		if (winter) {
			tileArray[mid.X - 2, mid.Y + 2][0] = OutdoorTiles[3, 15];
			tileArray[mid.X - 1, mid.Y + 2][0] = OutdoorTiles[1, 13];
			tileArray[mid.X, mid.Y + 2][0] = OutdoorTiles[1, 13];
			tileArray[mid.X + 1, mid.Y + 2][0] = OutdoorTiles[1, 13];
			tileArray[mid.X + 2, mid.Y + 2][0] = OutdoorTiles[3, 13];
		}

		var shadowTexture = Game1.shadowTexture;

		// insert Character's shadow
		tileArray[mid.X, mid.Y].Add(new(shadowTexture));

		// insert Character
		if (CenterCharacterTexture.Size.Height == 16) {
			tileArray[mid.X, mid.Y].Add(new(CenterCharacterTexture, offset: -32));
		}
		else {
			tileArray[mid.X, mid.Y - 1].Add(CenterCharacterTexture);
		}

		// insert Tree
		tileArray[mid.X - 4, mid.Y].Add(TreeTexture[1, 0]);

		var drawableInstances = new List<DrawableInstance>();

		for (int y = 0; y < tileCount.Y; ++y) {
			int yOffset = start.Y + (y * spriteSize.Y);

			for (int x = 0; x < tileCount.X; ++x) {
				int xOffset = start.X + (x * spriteSize.X);
				Vector2I offset = (xOffset, yOffset);

				var list = tileArray[x, y];

				foreach (var drawable in list) {
					drawableInstances.Add(new(drawable.Clone(rand), offset));
				}
			}
		}

		return drawableInstances.ToArray();
	}

	protected override void OnResize(Vector2I Size, Vector2I OldSize) {
		Drawables = SetupScene(Season == "winter");
	}

	protected override void OnTick() {
		CenterCharacterTexture.Tick();
	}
}
