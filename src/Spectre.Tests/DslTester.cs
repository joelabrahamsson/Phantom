#region License

// Copyright 2009 Jeremy Skinner (http://www.jeremyskinner.co.uk)
//  
// Licensed under the Apache License, Version 2.0 (the "License"); 
// you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at 
//  
// http://www.apache.org/licenses/LICENSE-2.0 
//  
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, 
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
// See the License for the specific language governing permissions and 
// limitations under the License.
// 
// The latest version of this file can be found at http://github.com/JeremySkinner/Spectre

#endregion

namespace Spectre.Tests {
	using System.Linq;
	using Core;
	using NUnit.Framework;

	[TestFixture]
	public class DslTester {
		[Test]
		public void Loads_simple_script() {
			string path = "Scripts\\SingleTarget.boo";
			var runner = new BuildRunner();
			var script = runner.GenerateBuildScript(path);
			script.Count().ShouldEqual(1);
		}

		[Test]
		public void Retrieves_target_name() {
			string path = "Scripts\\SingleTarget.boo";
			var runner = new BuildRunner();
			var script = runner.GenerateBuildScript(path);
			script.GetTarget("default").Name.ShouldEqual("default");
		}
	}
}