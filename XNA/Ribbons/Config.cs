using System.Collections.Generic;
using System.Drawing;
using System.Xml.Serialization;
using Microsoft.Xna.Framework.Graphics;

namespace Ribbons
{
	[XmlRoot("RibbonConfig")]
	public class Config
	{
		[XmlElement("BlobTimeout")]
		public double BlobTimeout = 30.0;

		[XmlElement("RibbonStartType")]
		public Game.RibbonType RibbonStartType = Game.RibbonType.ERibbonPoly;

		[XmlElement("ShowFramerate")]
		public bool ShowFramerate;

		[XmlElement("MaxRibbons")]
		public int MaxRibbons = 40;

		private Dictionary<int, Microsoft.Xna.Framework.Graphics.Color> colorCache = new Dictionary<int, Microsoft.Xna.Framework.Graphics.Color>();

		[XmlElement("Color_1_1")]
		public string Color_1_1 = "#78094b";

		[XmlElement("Color_1_2")]
		public string Color_1_2 = "#7e56b7";

		[XmlElement("Color_1_3")]
		public string Color_1_3 = "#518bd4";

		[XmlElement("Color_1_4")]
		public string Color_1_4 = "#18bd81";

		[XmlElement("Color_1_5")]
		public string Color_1_5 = "#c99100";

		[XmlElement("Color_1_6")]
		public string Color_1_6 = "#897455";

		[XmlElement("Color_1_7")]
		public string Color_1_7 = "#d63113";

		[XmlElement("Color_1_8")]
		public string Color_1_8 = "#f6f463";

		[XmlElement("Color_2_1")]
		public string Color_2_1 = "#7f848a";

		[XmlElement("Color_2_2")]
		public string Color_2_2 = "#94cec0";

		[XmlElement("Color_2_3")]
		public string Color_2_3 = "#7f9ac5";

		[XmlElement("Color_2_4")]
		public string Color_2_4 = "#62ecb9";

		[XmlElement("Color_2_5")]
		public string Color_2_5 = "#dbb14b";

		[XmlElement("Color_2_6")]
		public string Color_2_6 = "#adc6ef";

		[XmlElement("Color_2_7")]
		public string Color_2_7 = "#b982aa";

		[XmlElement("Color_2_8")]
		public string Color_2_8 = "#f4f3a0";

		[XmlElement("Color_3_1")]
		public string Color_3_1 = "#e2eab7";

		[XmlElement("Color_3_2")]
		public string Color_3_2 = "#8372fe";

		[XmlElement("Color_3_3")]
		public string Color_3_3 = "#c887ff";

		[XmlElement("Color_3_4")]
		public string Color_3_4 = "#13fcdf";

		[XmlElement("Color_3_5")]
		public string Color_3_5 = "#e96e44";

		[XmlElement("Color_3_6")]
		public string Color_3_6 = "#6cdcf0";

		[XmlElement("Color_3_7")]
		public string Color_3_7 = "#b6e500";

		[XmlElement("Color_3_8")]
		public string Color_3_8 = "#ffb500";

		[XmlElement("BackgroundColor")]
		public string BackgroundColor = "#000000";

		public Microsoft.Xna.Framework.Graphics.Color GetColor(int set, int i)
		{
			Microsoft.Xna.Framework.Graphics.Color value;
			if (colorCache.TryGetValue(i, out value))
			{
				return value;
			}
			string htmlColor = (string)GetType().GetField("Color_" + set + "_" + i).GetValue(this);
			System.Drawing.Color color = ColorTranslator.FromHtml(htmlColor);
			colorCache[i] = new Microsoft.Xna.Framework.Graphics.Color(color.R, color.G, color.B, color.A);
			return colorCache[i];
		}

		public void ClearCache()
		{
			colorCache.Clear();
		}

		public Microsoft.Xna.Framework.Graphics.Color GetBackgroundColor()
		{
			string htmlColor = (string)GetType().GetField("BackgroundColor").GetValue(this);
			System.Drawing.Color color = ColorTranslator.FromHtml(htmlColor);
			return new Microsoft.Xna.Framework.Graphics.Color(color.R, color.G, color.B, color.A);
		}
	}
}
