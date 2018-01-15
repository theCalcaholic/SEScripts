using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading;
using System.Globalization;
using IngameScript;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEScripts.XUI.Tests
{
	[TestClass()]
	public class RenderBoxLeafTests
	{
		Random Rnd;

		public RenderBoxLeafTests() : base()
		{
			Rnd = new Random();
		}

		private Program.RenderBoxLeaf GetTestLeaf(string content)
		{
			return new Program.RenderBoxLeaf(content);
		}

		private Program.RenderBoxLeaf GetTestLeaf()
		{
			var pool = "abcdefghijklmnopqrstuvwxyz0123456789";
			var length = Rnd.Next(4, 100);
			var builder = new StringBuilder();
			for(int i = 0; i < length; i++)
			{
				builder.Append(pool[Rnd.Next(0, pool.Length)]);
			}
			return GetTestLeaf(builder.ToString());
		}

		[TestInitialize()]
		public void Initialize()
		{
			Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
			// Do all rendering tests with mono space font for now. Other fonts might be dropped completely in the future.
			Program.TextUtils.SelectFont(Program.TextUtils.FONT.MONOSPACE);
			// Disable Caching for this test.
			Program.IRenderBox.CacheEnabled = false;
		}

		[TestMethod()]
		public void TestGetActualWidth()
		{
			Program.TextUtils.SelectFont(Program.TextUtils.FONT.MONOSPACE);
			/* case 1:
			 *	- no (explicit nor implicit) max width defined
			 *	- no explicit min width defined
			 *	- no explicit desired width defined
			 *	
			 *	Expected: 25 (default min width)
			 */
			var leaf = GetTestLeaf();

			Assert.AreEqual(25, leaf.GetActualWidth(int.MaxValue));

			/* case 2:
			 *  - max width defined implicitly > text width
			 *  - no min width defined
			 *  - desired width defined < max width
			 *  
			 *  Expected: text width (text nodes are restrained to exact text width after initialization)
			 */

			leaf = GetTestLeaf("abcd"); // -> text width == 99
			var privateAccessor = new PrivateObject(leaf);
			leaf.DesiredWidth = 150;
			leaf.Initialize(5000, 5000);
			StringBuilder[] lineCache = privateAccessor.GetField("LineCache") as StringBuilder[];
			Console.WriteLine("string width: " + Program.TextUtils.GetTextWidth("abcd"));
			Assert.AreEqual(99, leaf.GetActualWidth(600));

			/* case 2.1:
			 *  - max width defined implicitly > text width
			 *  - no min width defined
			 *  - desired width defined < max width
			 *  
			 *  Expected: text width (text nodes are restrained to exact text width after initialization)
			 */

			leaf = GetTestLeaf("abcd"); // -> text width == 99
			leaf.DesiredWidth = 80;
			leaf.Initialize(5000, 5000);

			Assert.AreEqual(99, leaf.GetActualWidth(600));

			/* case 3:
			 *	- max width defined explicitly
			 *	- min width defined explicitly > max width
			 *	- desired width undefined
			 *	
			 *	Expected: explicit maximum width
			 */

			leaf = GetTestLeaf("abcd"); 
			leaf.MaxWidth = 120;
			leaf.MinWidth = 150;

			Assert.AreEqual(120, leaf.GetActualWidth(600));

			/* case 4:
			 *  - min width < max width < desired width
			 * 
			 * Expected: max width
			 */
			leaf = GetTestLeaf();
			leaf.DesiredWidth = 400;
			Assert.AreEqual(200, leaf.GetActualWidth(200));

			/* case 5:
			 *  - nothing defined explicitely.
			 *  - leaf has been initialized (text width is known);
			 *  
			 *  Expected: text width
			 */

			leaf = GetTestLeaf("abcdef"); // -> text width = 149
			leaf.Initialize(5000, 5000);
			Assert.AreEqual(149, leaf.GetActualWidth(1000));

			//TODO: Add more edge cases
		}

		[TestMethod()]
		public void TestGetActualHeight()
		{
			/* case 1:
			 *	- no (explicit nor implicit) max height defined
			 *	- no explicit min height defined
			 *	- no explicit desired height defined
			 *	
			 *	Expected: 1 (default min height if content length > 0)
			 */
			var leaf = GetTestLeaf();

			Assert.AreEqual(1, leaf.GetActualHeight(int.MaxValue));

			/* case 2:
			 *  - no (explicit nor implicit) max height defined
			 *	- no explicit min height defined
			 *	- no explicit desired height defined
			 *	- content = ""
			 *  
			 *  Expected: 0 (default min height if content length == 0)
			 */

			leaf = GetTestLeaf();
			var privateAccessor = new PrivateObject(leaf);
			privateAccessor.SetField("Content", "");

			Assert.AreEqual(0, leaf.GetActualHeight(int.MaxValue));

			/* case 3:
			 *  - max width defined
			 *  - content width > max width
			 *  - no min/max/desired height defined
			 *  
			 *  Expected: 2 (number of lines)
			 */

			leaf = GetTestLeaf();
			privateAccessor = new PrivateObject(leaf);
			privateAccessor.SetField("Content", "abcdef");
			privateAccessor.SetField("_MaxWidth", 75);
			leaf.Initialize(5000, 5000);

			Assert.AreEqual(2, leaf.GetActualHeight(int.MaxValue));

			//TODO: Add additional edge cases
		}

		[TestMethod()]
		public void TestFlow_Get()
		{
			var leaf = GetTestLeaf();
			Assert.AreEqual(Program.IRenderBox.FlowDirection.VERTICAL, leaf.Flow);
		}

		[TestMethod()]
		public void TestFlow_Set()
		{
			var leaf = GetTestLeaf();
			leaf.Flow = Program.IRenderBox.FlowDirection.HORIZONTAL;
			Assert.AreEqual(Program.IRenderBox.FlowDirection.VERTICAL, leaf.Flow);
		}

		[TestMethod()]
		public void TestMinWidth_Get()
		{
			var leaf = GetTestLeaf();
			var privateAccessor = new PrivateObject(leaf);
			var content = privateAccessor.GetField("Content") as string;

			// without container width and explicit max/min width set, should default to character width.
			Assert.AreEqual(leaf.MinWidth, 25);
			
			privateAccessor.SetField("_MinWidth", 100);
			privateAccessor.SetField("minWidthIsCached", false);

			// if min width is set (and > 25) and container width isn't, should equal explicit min width
			Assert.AreEqual(leaf.MinWidth, 100);

			privateAccessor.SetField("TextWidth", Program.TextUtils.GetTextWidth(content));
			privateAccessor.SetField("minWidthIsCached", false);

			// if text width has been calculated, should be maximum of min width, char width and text width
			Assert.AreEqual(leaf.MinWidth, Program.TextUtils.GetTextWidth(content));
		}

		[TestMethod()]
		public void TestMinWidth_Set()
		{
			var leaf = GetTestLeaf();
			var privateAccessor = new PrivateObject(leaf);

			privateAccessor.SetField("minWidthIsCached", true);
			leaf.MinWidth = 80;
			Assert.AreEqual(80, privateAccessor.GetField("_MinWidth"));
			Assert.AreEqual(false, privateAccessor.GetField("minWidthIsCached"));
		}

		[TestMethod()]
		public void TestDesiredWidth_Get()
		{
			var leaf = GetTestLeaf();
			var privateAccessor = new PrivateObject(leaf);

			Assert.AreEqual(privateAccessor.GetField("_DesiredWidth"), leaf.DesiredWidth);
			privateAccessor.SetField("_DesiredWidth", 120);
			Assert.AreEqual(privateAccessor.GetField("_DesiredWidth"), leaf.DesiredWidth);
		}

		[TestMethod()]
		public void TestDesiredWidth_Set()
		{
			var leaf = GetTestLeaf();
			var privateAccessor = new PrivateObject(leaf);

			leaf.DesiredWidth = 130;
			Assert.AreEqual(130, privateAccessor.GetField("_DesiredWidth"));
		}

		[TestMethod()]
		public void TestMaxWidth_Get()
		{
			var leaf = GetTestLeaf();
			var privateAccessor = new PrivateObject(leaf);
			var content = privateAccessor.GetField("Content") as string;

			// When uninitialized and no explicit max width given, should be int.maxvalue
			Assert.AreEqual(int.MaxValue, leaf.MaxWidth);

			privateAccessor.SetField("_MaxWidth", 1000);
			// If uninitialized should return the explicit max width
			Assert.AreEqual(1000, leaf.MaxWidth);

			privateAccessor.SetField("TextWidth", 100);
			// If text width has been calculated, should return text width
			Assert.AreEqual(100, leaf.MaxWidth);
		}

		[TestMethod()]
		public void TestMaxWidth_Set()
		{
			var leaf = GetTestLeaf();
			var privateAccessor = new PrivateObject(leaf);

			leaf.MaxWidth = 10;
			Assert.AreEqual(10, privateAccessor.GetField("_MaxWidth"));

			// not possible to set negative values. They disable max width instead.
			leaf.MaxWidth = -10;
			Assert.AreEqual(int.MaxValue, privateAccessor.GetField("_MaxWidth"));
		}

		[TestMethod()]
		public void TestMinHeight_Get()
		{
			var leaf = GetTestLeaf();
			var privateAccessor = new PrivateObject(leaf);
			var content = privateAccessor.GetField("Content") as string;
			
			// should be maximum of explicit min height and 1 if content length is > 0 leaf is not initialized.
			Assert.AreEqual(1, leaf.MinHeight);
			privateAccessor.SetField("_MinHeight", 5);
			Assert.AreEqual(5, leaf.MinHeight);

			privateAccessor.SetField("Content", "");
			// should equal explicit min height if content length is 0 and leaf is not initialized.
			Assert.AreEqual(5, leaf.MinHeight);
			privateAccessor.SetField("_MinHeight", 0);
			Assert.AreEqual(0, leaf.MinHeight);

			privateAccessor.SetField("Content", "abcabc");
			leaf.Initialize(75, int.MaxValue);

			// should be maximum of <number of lines> and minheight when leaf is initialized.
			Assert.AreEqual(2, leaf.MinHeight);
		}

		[TestMethod()]
		public void TestMinHeigth_Set()
		{
			var leaf = GetTestLeaf();
			var privateAccessor = new PrivateObject(leaf);

			privateAccessor.SetField("minHeightIsCached", true);
			leaf.MinHeight = 12;
			// should have set explicit min height value.
			Assert.AreEqual(12, privateAccessor.GetField("_MinHeight"));
			// should have reset min height cache.
			Assert.IsFalse((bool) privateAccessor.GetField("minHeightIsCached"));

		}

		[TestMethod()]
		public void TestDesiredHeight_Get()
		{
			var leaf = GetTestLeaf();
			var privateAccessor = new PrivateObject(leaf);

			// should return explicit value if not initialized.
			Assert.AreEqual(privateAccessor.GetField("_DesiredHeight"), leaf.DesiredHeight);

			leaf.Initialize(1000, 1000);

			// should return calculated, dynamic height if initialized.
			Assert.AreEqual(privateAccessor.GetField("DynamicHeight"), leaf.DesiredHeight);
		}

		[TestMethod()]
		public void TestDesiredHeight_Set()
		{
			var leaf = GetTestLeaf();
			var privateAccessor = new PrivateObject(leaf);

			leaf.DesiredHeight = 12;
			Assert.AreEqual(12, privateAccessor.GetField("_DesiredHeight"));
		}

		[TestMethod()]
		public void TestMaxHeight_Get()
		{
			var leaf = GetTestLeaf();
			var privateAccessor = new PrivateObject(leaf);

			privateAccessor.SetField("_MaxHeight", 23);
			Assert.AreEqual(23, leaf.MaxHeight);
		}

		[TestMethod()]
		public void TestMaxHeight_Set()
		{
			var leaf = GetTestLeaf();
			var privateAccessor = new PrivateObject(leaf);

			leaf.MaxHeight = 10;
			Assert.AreEqual(10, privateAccessor.GetField("_MaxHeight"));

			// not possible to set negative values. They disable max width instead.
			leaf.MaxHeight = -10;
			Assert.AreEqual(int.MaxValue, privateAccessor.GetField("_MaxHeight"));
		}

		[TestMethod()]
		public void TestGetLines()
		{
			var content = "asd09ha ahjas0dh912l aosido2 as";
			var leaf = GetTestLeaf(content);
			var privateAccessor = new PrivateObject(leaf);

			leaf.Initialize(299, 5000);
			var lineCache = privateAccessor.GetField("LineCache") as StringBuilder[];
			var lines = leaf.GetLines(299, 5000).ToList();
			Assert.AreEqual(lineCache.Length, lines.Count);
			for (int i = 0; i < lineCache.Length; i++)
			{
				Assert.AreEqual(lineCache[i], lines[i]);
			}
		}

		[TestMethod()]
		public void TestAlignLine()
		{
			var content = "abcdef";
			var leaf = new Program.RenderBoxLeaf(content);
			leaf.Initialize(5000, 5000);
			var privateAccessor = new PrivateObject(leaf);
			var actualWidth = leaf.GetActualWidth(1000);
			Console.WriteLine("GetActualWidth(1000) -> " + actualWidth.ToString());
			var args = new object[] { new StringBuilder(content), actualWidth, Program.IRenderBox.TextAlign.LEFT, ' ' };
			privateAccessor.Invoke("AlignLine", args);
			var aligned = args[0].ToString();
			// content should remain unchanged if max width == content width
			Assert.AreEqual(content, aligned.ToString());

			/** alignment by padding **/

			args = new object[] { new StringBuilder(content), 207, Program.IRenderBox.TextAlign.LEFT, ' ' };
			privateAccessor.Invoke("AlignLine", args);
			aligned = args[0].ToString();
			Assert.AreEqual(
				Program.TextUtils.PadText(content, 207, Program.TextUtils.PadMode.RIGHT).ToString(),
				aligned);

			args = new object[] { new StringBuilder(content), 207, Program.IRenderBox.TextAlign.RIGHT, ' ' };
			privateAccessor.Invoke("AlignLine", args);
			aligned = args[0].ToString();
			Assert.AreEqual(
				Program.TextUtils.PadText(content, 207, Program.TextUtils.PadMode.LEFT).ToString(),
				aligned);

			args = new object[] { new StringBuilder(content), 207, Program.IRenderBox.TextAlign.CENTER, ' ' };
			privateAccessor.Invoke("AlignLine", args);
			aligned = args[0].ToString();
			Assert.AreEqual(
				Program.TextUtils.PadText(content, 207, Program.TextUtils.PadMode.BOTH).ToString(),
				aligned);

			/** alignment by clipping **/

			args = new object[] { new StringBuilder(content), 99, Program.IRenderBox.TextAlign.LEFT, ' ' };
			privateAccessor.Invoke("AlignLine", args);
			aligned = args[0].ToString();
			Assert.AreEqual(
				"abcd",
				aligned);

			args = new object[] { new StringBuilder(content), 99, Program.IRenderBox.TextAlign.RIGHT, ' ' };
			privateAccessor.Invoke("AlignLine", args);
			aligned = args[0].ToString();
			Assert.AreEqual(
				"abcd",
				aligned);

			args = new object[] { new StringBuilder(content), 103, Program.IRenderBox.TextAlign.CENTER, ' ' };
			privateAccessor.Invoke("AlignLine", args);
			aligned = args[0].ToString();
			Assert.AreEqual(
				"abcd",
				aligned);
		}

		[TestMethod()]
		public void TestRender()
		{
			var content = "asd09ha ahjas0dh912l aosido2 as";
			var leaf = GetTestLeaf(content);
			var privateAccessor = new PrivateObject(leaf);
			var result = leaf.Render(299, 5000);
			var lineCache = privateAccessor.GetField("LineCache") as StringBuilder[];
			var expected = String.Join<StringBuilder>("\n", lineCache);

			Assert.AreEqual(expected, result);
		}

		[TestMethod()]
		public void TestClearCache()
		{
			Program.IRenderBox.CacheEnabled = true;
			var leaf = GetTestLeaf();
			var leafPrivateAccessor = new PrivateObject(leaf);
			var tree = new Program.RenderBoxTree();
			var treePrivateAccessor = new PrivateObject(tree);
			tree.Add(leaf);

			leafPrivateAccessor.SetField("minHeightIsCached", true);
			leafPrivateAccessor.SetField("minWidthIsCached", true);
			leafPrivateAccessor.SetField("desiredWidthIsCached", true);
			leafPrivateAccessor.SetField("desiredHeightIsCached", true);
			treePrivateAccessor.SetField("minHeightIsCached", true);
			treePrivateAccessor.SetField("minWidthIsCached", true);
			treePrivateAccessor.SetField("desiredWidthIsCached", true);
			treePrivateAccessor.SetField("desiredHeightIsCached", true);

			leaf.ClearCache();

			Assert.IsFalse((bool) leafPrivateAccessor.GetField("minHeightIsCached"));
			Assert.IsFalse((bool) leafPrivateAccessor.GetField("minWidthIsCached"));
			Assert.IsFalse((bool) leafPrivateAccessor.GetField("desiredHeightIsCached"));
			Assert.IsFalse((bool) leafPrivateAccessor.GetField("desiredWidthIsCached"));
			Assert.IsFalse((bool) treePrivateAccessor.GetField("minHeightIsCached"));
			Assert.IsFalse((bool) treePrivateAccessor.GetField("minWidthIsCached"));
			Assert.IsFalse((bool) treePrivateAccessor.GetField("desiredHeightIsCached"));
			Assert.IsFalse((bool) treePrivateAccessor.GetField("desiredWidthIsCached"));
		}

		[TestMethod()]
		public void TestAdd()
		{
			var leaf = new Program.RenderBoxLeaf();
			leaf.Add("test");
			Assert.AreEqual("test", leaf.Content);

			leaf = new Program.RenderBoxLeaf();
			leaf.Add(new StringBuilder("test"));
			Assert.AreEqual("test", leaf.Content);
		}

		[TestMethod()]
		public void TestAddAt()
		{
			var leaf = GetTestLeaf();
			leaf.AddAt(1, "test");
			Assert.IsTrue(leaf.Content.EndsWith("test"));

			leaf = GetTestLeaf();
			leaf.AddAt(0, "test");
			Assert.IsTrue(leaf.Content.StartsWith("test"));
		}

		[TestMethod()]
		public void TestGetLine()
		{
			var content = "asd09ha ahjas0dh912l aosido2 as";
			var leaf = GetTestLeaf(content);
			var privateAccessor = new PrivateObject(leaf);

			leaf.Initialize(299, 5000);
			var lineCache = privateAccessor.GetField("LineCache") as StringBuilder[];
			Assert.AreEqual(lineCache[0], leaf.GetLine(0));
			Assert.AreEqual(lineCache[1], leaf.GetLine(1));
			Assert.AreEqual(lineCache[2], leaf.GetLine(2));
		}

		[TestMethod()]
		public void TestClear()
		{
			var leaf = GetTestLeaf();
			var privateAccessor = new PrivateObject(leaf);

			leaf.Initialize(5000, 5000);
			leaf.Clear();

			Assert.AreEqual("", privateAccessor.GetField("Content"));
			Assert.AreEqual(-1, privateAccessor.GetField("DynamicHeight"));
			Assert.AreEqual(0, privateAccessor.GetField("TextWidth"));
			Assert.IsNull(privateAccessor.GetField("LineCache"));
			TestClearCache();
		}

		[TestMethod()]
		public void TestInitialize()
		{
			var content = "abcdef";
			var leaf = GetTestLeaf(content);
			var privateAccessor = new PrivateObject(leaf);

			leaf.Initialize(5000, 5000);

			Assert.IsTrue(leaf.Initialized);
			Assert.AreEqual(1, privateAccessor.GetField("DynamicHeight"));
			Assert.AreEqual(1, leaf.MinHeight);
			Assert.AreEqual(1, leaf.DesiredHeight);
			Assert.AreEqual(Program.TextUtils.GetTextWidth(content), privateAccessor.GetField("TextWidth"));
			Assert.AreEqual(Program.TextUtils.GetTextWidth(content), leaf.MinWidth);
			Assert.AreEqual(Program.TextUtils.GetTextWidth(content), leaf.MaxWidth);

			content = "abcdef igf ai0einls a";
			leaf = GetTestLeaf(content);
			privateAccessor = new PrivateObject(leaf);

			leaf.Initialize(154, 5000);

			Assert.IsTrue(leaf.Initialized);
			Assert.AreEqual(4, privateAccessor.GetField("DynamicHeight"));
			Assert.AreEqual(4, leaf.MinHeight);
			Assert.AreEqual(4, leaf.DesiredHeight);
			Assert.AreEqual(149, privateAccessor.GetField("TextWidth"));
			Assert.AreEqual(149, leaf.MinWidth);
			Assert.AreEqual(149, leaf.MaxWidth);
		}

		[TestMethod()]
		public void TestRenderBoxLeaf()
		{
			var leaf = new Program.RenderBoxLeaf();
			var privateAccessor = new PrivateObject(leaf);

			Assert.AreEqual(leaf.Content, "");
			Assert.AreEqual(-1, privateAccessor.GetField("DynamicHeight"));
			Assert.AreEqual(0, privateAccessor.GetField("TextWidth"));
			Assert.IsNull(privateAccessor.GetField("LineCache"));
			Assert.IsFalse((bool)privateAccessor.GetField("minHeightIsCached"));
			Assert.IsFalse((bool)privateAccessor.GetField("minWidthIsCached"));
			Assert.IsFalse((bool)privateAccessor.GetField("desiredWidthIsCached"));
			Assert.IsFalse((bool)privateAccessor.GetField("desiredHeightIsCached"));
		}

		[TestMethod()]
		public void TestBuildLineCache()
		{
			var leaf = GetTestLeaf();
			var privateAccessor = new PrivateObject(leaf);

			privateAccessor.Invoke("BuildLineCache", new object[] { 5000, 5000 });

			var lineCache = privateAccessor.GetField("LineCache") as StringBuilder[];

			Assert.AreEqual(1, lineCache.Length);
			Assert.AreEqual(leaf.Content, lineCache[0].ToString());

			var content = "asdnoih2 aihb 3 ahi3 asa";
			leaf = GetTestLeaf(content);
			privateAccessor = new PrivateObject(leaf);

			privateAccessor.Invoke("BuildLineCache", new object[] { 99, 5000 });

			lineCache = privateAccessor.GetField("LineCache") as StringBuilder[];

			Assert.AreEqual(6, lineCache.Length);
			Assert.AreEqual(content.Substring(0, 4), lineCache[0].ToString());
			Assert.AreEqual(content.Substring(4, 4), lineCache[1].ToString());
			Assert.AreEqual(content.Substring(9, 4), lineCache[2].ToString());
			Assert.AreEqual(content.Substring(14, 1), lineCache[3].ToString());
			Assert.AreEqual(content.Substring(16, 4), lineCache[4].ToString());
			Assert.AreEqual(content.Substring(21), lineCache[5].ToString());
		}
	}
}
