using Microsoft.VisualStudio.TestTools.UnitTesting;
using SEScripts.MultiAgentFramework.MAF.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEScripts.MultiAgentFramework.MAF.Models.Tests
{
    [TestClass()]
    public class AgentMessageTests
    {
        [TestMethod()]
        public void ToStringTest()
        {
            AgentMessage m = new AgentMessage(new AgentId("sender"), new AgentId("receiver"), "hello");
            string message = m.ToString();
            string expected = "<message sender=\"sender@local\" receiver=\"receiver@local\" content=\"hello\"/>";
            if(message != expected)
            {
                Assert.Fail();
            }
        }
    }
}