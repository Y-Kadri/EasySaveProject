﻿// ------------------------------------------------------------------------------
//  <auto-generated>
//      This code was generated by SpecFlow (https://www.specflow.org/).
//      SpecFlow Version:3.9.0.0
//      SpecFlow Generator Version:3.9.0.0
// 
//      Changes to this file may cause incorrect behavior and will be lost if
//      the code is regenerated.
//  </auto-generated>
// ------------------------------------------------------------------------------
#region Designer generated code
#pragma warning disable
namespace EasySave_Project_Test.Features.ProjectInitializationSteps
{
    using TechTalk.SpecFlow;
    using System;
    using System.Linq;
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "3.9.0.0")]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [NUnit.Framework.TestFixtureAttribute()]
    [NUnit.Framework.DescriptionAttribute("Project Initialization - Ensure Jobs Creation - Load Jobs from jobsSetting.json")]
    public partial class ProjectInitialization_EnsureJobsCreation_LoadJobsFromJobsSetting_JsonFeature
    {
        
        private TechTalk.SpecFlow.ITestRunner testRunner;
        
        private static string[] featureTags = ((string[])(null));
        
#line 1 "EnsureJobsCreation.feature"
#line hidden
        
        [NUnit.Framework.OneTimeSetUpAttribute()]
        public virtual void FeatureSetup()
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en"), "Features/ProjectInitializationSteps", "Project Initialization - Ensure Jobs Creation - Load Jobs from jobsSetting.json", "  As a user of the EasySave application,\r\n  I want to ensure that the jobs are co" +
                    "rrectly loaded from the \"jobsSetting.json\" file,\r\n  So that the application can " +
                    "properly manage the job data.", ProgrammingLanguage.CSharp, featureTags);
            testRunner.OnFeatureStart(featureInfo);
        }
        
        [NUnit.Framework.OneTimeTearDownAttribute()]
        public virtual void FeatureTearDown()
        {
            testRunner.OnFeatureEnd();
            testRunner = null;
        }
        
        [NUnit.Framework.SetUpAttribute()]
        public void TestInitialize()
        {
        }
        
        [NUnit.Framework.TearDownAttribute()]
        public void TestTearDown()
        {
            testRunner.OnScenarioEnd();
        }
        
        public void ScenarioInitialize(TechTalk.SpecFlow.ScenarioInfo scenarioInfo)
        {
            testRunner.OnScenarioInitialize(scenarioInfo);
            testRunner.ScenarioContext.ScenarioContainer.RegisterInstanceAs<NUnit.Framework.TestContext>(NUnit.Framework.TestContext.CurrentContext);
        }
        
        public void ScenarioStart()
        {
            testRunner.OnScenarioStart();
        }
        
        public void ScenarioCleanup()
        {
            testRunner.CollectScenarioErrors();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Verify that jobs are loaded from the jobsSetting.json file if it exists")]
        public void VerifyThatJobsAreLoadedFromTheJobsSetting_JsonFileIfItExists()
        {
            string[] tagsOfScenario = ((string[])(null));
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Verify that jobs are loaded from the jobsSetting.json file if it exists", null, tagsOfScenario, argumentsOfScenario, featureTags);
#line 7
  this.ScenarioInitialize(scenarioInfo);
#line hidden
            if ((TagHelper.ContainsIgnoreTag(tagsOfScenario) || TagHelper.ContainsIgnoreTag(featureTags)))
            {
                testRunner.SkipScenario();
            }
            else
            {
                this.ScenarioStart();
#line 8
    testRunner.Given("the \"jobsSetting.json\" file exists in the \"easySave/easySaveSetting\" directory wi" +
                        "th the following content:", @"{
  ""jobs"": [
    {
      ""Id"": 1,
      ""SaveState"": ""INACTIVE"",
      ""SaveType"": ""COMPLETE"",
      ""Name"": ""Backup Documents"",
      ""FileSource"": ""C:\\Documents\\myfile.txt"",
      ""FileTarget"": ""D:\\Backup\\myfile.txt"",
      ""FileSize"": ""1024"",
      ""FileTransferTime"": ""30"",
      ""Time"": ""2025-02-03T12:00:00""
    }
  ],
  ""index"": 1
}", ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line hidden
#line 27
    testRunner.When("I load the jobs from the \"jobsSetting.json\" file", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
                TechTalk.SpecFlow.Table table1 = new TechTalk.SpecFlow.Table(new string[] {
                            "Property",
                            "Value"});
                table1.AddRow(new string[] {
                            "Id",
                            "1"});
                table1.AddRow(new string[] {
                            "SaveState",
                            "INACTIVE"});
                table1.AddRow(new string[] {
                            "SaveType",
                            "COMPLETE"});
                table1.AddRow(new string[] {
                            "Name",
                            "Backup Documents"});
                table1.AddRow(new string[] {
                            "FileSource",
                            "C:\\Documents\\myfile.txt"});
                table1.AddRow(new string[] {
                            "FileTarget",
                            "D:\\Backup\\myfile.txt"});
                table1.AddRow(new string[] {
                            "FileSize",
                            "1024"});
                table1.AddRow(new string[] {
                            "FileTransferTime",
                            "30"});
                table1.AddRow(new string[] {
                            "Time",
                            "2025-02-03T12:00:00"});
#line 28
    testRunner.Then("the job with id 1 should be loaded with the following details:", ((string)(null)), table1, "Then ");
#line hidden
            }
            this.ScenarioCleanup();
        }
    }
}
#pragma warning restore
#endregion
