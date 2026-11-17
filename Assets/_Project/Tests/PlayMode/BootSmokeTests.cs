using System.Collections;
using NUnit.Framework;
using Shrine.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Shrine.Tests
{
    public sealed class BootSmokeTests
    {
        [UnityTest]
        public IEnumerator ShrineScene_BootsToTitleWithoutErrors()
        {
            yield return SceneManager.LoadSceneAsync("Shrine");
            yield return null;
            yield return null;

            ShrineApp app = Object.FindFirstObjectByType<ShrineApp>();
            Assert.IsNotNull(app, "ShrineApp must be present in the Shrine scene.");
            Assert.AreEqual("Title", app.Router.Current, "The game must open on the title screen.");
            Assert.IsNotNull(GameObject.Find("Canvas"), "Runtime UI canvas must exist.");
            Assert.IsTrue(GameObject.Find("TitleRoot").activeSelf, "Title screen must be active.");
        }

        [UnityTest]
        public IEnumerator ShrineScene_CanReachCovenant()
        {
            yield return SceneManager.LoadSceneAsync("Shrine");
            yield return null;
            ShrineApp app = Object.FindFirstObjectByType<ShrineApp>();
            app.Router.Show("Covenant");
            yield return null;
            Assert.AreEqual("Covenant", app.Router.Current);
            Assert.IsTrue(GameObject.Find("CovenantRoot").activeSelf);
        }
    }
}
