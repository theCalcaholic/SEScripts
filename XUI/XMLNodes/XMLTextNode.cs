using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System;
using VRage.Collections;
using VRage.Game.Components;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ObjectBuilders.Definitions;
using VRage.Game;
using VRageMath;

namespace IngameScript
{
	partial class Program
	{
		public class XMLTextNode : XMLTree
		{
			public string Content;

			public XMLTextNode(string content) : base()
			{
				//Logger.debug("TextNode constructor()");
				//Logger.IncLvl();
				//Logger.debug("content: " + content);
				Type = "textnode";
				Content = content;
				Content.Replace("\n", "");
				Content = Content.Trim(new char[] { '\n', ' ', '\r' });
				//SetAttribute("width", "100%");
				//Logger.debug("final content: " + Content);
				RerenderRequired = true;
				//Logger.DecLvl();
			}

			public override IRenderBox GetRenderBox(int containerWidth, int containerHeight)
			{
				//using (new Logger("XMLTree<" + Type + ">.GetRenderBox(int, int)"))
				//{
				//Logger.debug("TextNode.GetRenderCache(int)");
				//Logger.IncLvl();
				IRenderBox cache = new RenderBoxLeaf(Content);
				cache.type = Type;
				UpdateRenderCacheProperties(cache, containerWidth, containerHeight);
				//Logger.DecLvl();
				return cache;
				//}
			}

			//protected override void RenderText(ref List<string> segments, int width, int availableWidth) { }

			/*protected override string PostRender(List<string> segments, int width, int availableWidth)
			{
				return Content;
			}*/


			/*protected override void BuildRenderCache()
			{
				//Logger.debug(Type + ".BuildRenderCache()");
				//Logger.IncLvl();
				RerenderRequired = false;
				//Logger.DecLvl();
			}*/
		}

		//EMBED SEScripts.XUI.XML.XMLTree
	}
}