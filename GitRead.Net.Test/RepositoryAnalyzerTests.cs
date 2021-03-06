﻿using System.Collections.Generic;
using System.Linq;
using GitRead.Net.Data;
using NUnit.Framework;

namespace GitRead.Net.Test
{
    [TestFixture]
    public class RepositoryAnalyzerTests
    {
        [Test]
        public void TestCountCommits()
        {
            string repoDir = TestUtils.ExtractZippedRepo("csharplang.git");
            RepositoryAnalyzer repositoryAnalyzer = new RepositoryAnalyzer(repoDir);
            int count = repositoryAnalyzer.GetTotalNumberOfCommits();
            Assert.AreEqual(157, count);
        }

        [Test]
        public void TestGetCommits()
        {
            string repoDir = TestUtils.ExtractZippedRepo("csharplang.git");
            RepositoryAnalyzer repositoryAnalyzer = new RepositoryAnalyzer(repoDir);
            List<string> commits = repositoryAnalyzer.GetCommits().Select(x => x.Hash).ToList();
            Assert.Less(commits.IndexOf("f985e74e689d6857daca1141564dfbc6fd658b08"), commits.IndexOf("460058747f22757b61b8a4f5ad6beb1c2043eef4")); //4600 is a parent of f985
            Assert.Less(commits.IndexOf("380631f0cc7dcc56fdd4af27d77cb0df01c1478c"), commits.IndexOf("460058747f22757b61b8a4f5ad6beb1c2043eef4")); //4600 is a parent of 3806
            Assert.Less(commits.IndexOf("fc7e0f6d81c71944d68cd798bf1d85c9decbc59f"), commits.IndexOf("460058747f22757b61b8a4f5ad6beb1c2043eef4")); //4600 is a parent of fc7e
            Assert.Less(commits.IndexOf("c5de26f52019ca0e1e79d88584f75bfe530ee986"), commits.IndexOf("3524ad4e2bd13ce56def7a4e16986215dd051363")); //3524 is a parent of c5de
            Assert.Less(commits.IndexOf("c5de26f52019ca0e1e79d88584f75bfe530ee986"), commits.IndexOf("e23c9e0ebdcef2a5ceafa16ce7b652335edd5c41")); //e23c is a parent of c5de
            Assert.Less(commits.IndexOf("a275da6616b2d66ae878d894940e79c5e96a3b84"), commits.IndexOf("564b51a481968cebc0c2a8b83e0da1bf652fc9db")); //both share parent but 564b has timetamp which is before timestamp of a275
            Assert.AreEqual("f23184baaccbd403436c6a705f4fc06f90df3086", commits[commits.Count - 1]); // f231 is the commit which started the repository
        }

        [Test]
        public void TestGetFilePathsHead()
        {
            string repoDir = TestUtils.ExtractZippedRepo("csharplang.git");
            RepositoryAnalyzer repositoryAnalyzer = new RepositoryAnalyzer(repoDir);
            List<string> filePaths = repositoryAnalyzer.GetFilePaths().ToList();
            Assert.AreEqual(152, filePaths.Count);
            Assert.True(filePaths.Contains(".gitattributes"));
            Assert.True(filePaths.Contains(@"meetings\README.md"));
            Assert.True(filePaths.Contains(@"proposals\rejected\README.md"));
        }

        [Test]
        public void TestGetFilePathsSpecificCommit()
        {
            string repoDir = TestUtils.ExtractZippedRepo("csharplang.git");
            RepositoryAnalyzer repositoryAnalyzer = new RepositoryAnalyzer(repoDir);
            List<string> filePaths = repositoryAnalyzer.GetFilePaths("7981ea1fb4d89571fd41e3b75f7c9f7fc178e837").ToList();
            Assert.AreEqual(6, filePaths.Count);
            Assert.True(filePaths.Contains("README.md"));
            Assert.True(filePaths.Contains(@"design-notes\Notes-2016-11-16.md"));
            Assert.True(filePaths.Contains(@"design-notes\csharp-language-design-notes-2017.md"));
            Assert.True(filePaths.Contains(@"proposals\async-streams.md"));
            Assert.True(filePaths.Contains(@"proposals\nullable-reference-types.md"));
            Assert.True(filePaths.Contains(@"spec\spec.md"));
        }

        [Test]
        public void TestGetChangesByCommitOneParentFilesAdded()
        {
            string repoDir = TestUtils.ExtractZippedRepo("csharplang.git");
            RepositoryAnalyzer repositoryAnalyzer = new RepositoryAnalyzer(repoDir);
            CommitDelta changes = repositoryAnalyzer.GetChanges("7981ea1fb4d89571fd41e3b75f7c9f7fc178e837");
            Assert.AreEqual(2, changes.Added.Count);
            Assert.AreEqual(0, changes.Deleted.Count);
            Assert.AreEqual(0, changes.Modified.Count);
            FileChange nullableChanges = changes.Added.Where(x => x.Path == @"proposals\nullable-reference-types.md").First();
            Assert.AreEqual(0, nullableChanges.NumberOfLinesDeleted);
            Assert.AreEqual(126, nullableChanges.NumberOfLinesAdded);
            FileChange notesChanges = changes.Added.Where(x => x.Path == @"design-notes\Notes-2016-11-16.md").First();
            Assert.AreEqual(0, notesChanges.NumberOfLinesDeleted);
            Assert.AreEqual(74, notesChanges.NumberOfLinesAdded);
        }

        [Test]
        public void TestGetChangesByCommitOneParentFilesModified()
        {
            string repoDir = TestUtils.ExtractZippedRepo("csharplang.git");
            RepositoryAnalyzer repositoryAnalyzer = new RepositoryAnalyzer(repoDir);
            CommitDelta changes = repositoryAnalyzer.GetChanges("a5f82604eab5826bd1913cf63c7dfb8c2b187641");
            Assert.AreEqual(0, changes.Added.Count);
            Assert.AreEqual(0, changes.Deleted.Count);
            Assert.AreEqual(2, changes.Modified.Count);
            FileChange readmeChanges = changes.Modified.Where(x => x.Path == @"README.md").First();
            Assert.AreEqual(4, readmeChanges.NumberOfLinesDeleted);
            Assert.AreEqual(12, readmeChanges.NumberOfLinesAdded);
            FileChange proposalsReadmeChanges = changes.Modified.Where(x => x.Path == @"proposals\README.md").First();
            Assert.AreEqual(1, proposalsReadmeChanges.NumberOfLinesAdded);
            Assert.AreEqual(1, proposalsReadmeChanges.NumberOfLinesDeleted);
        }

        [Test]
        public void TestGetChangesByCommitTwoParentsNoChange()
        {
            string repoDir = TestUtils.ExtractZippedRepo("csharplang.git");
            RepositoryAnalyzer repositoryAnalyzer = new RepositoryAnalyzer(repoDir);
            CommitDelta changes = repositoryAnalyzer.GetChanges("dfb46ac110aacfade7a4a9491b272e6e8ffc4468");
            Assert.AreEqual(0, changes.Added.Count);
            Assert.AreEqual(0, changes.Deleted.Count);
            Assert.AreEqual(0, changes.Modified.Count);
        }
        
        [Test]
        public void TestGetChangesByCommitTwoParentsWithChange()
        {
            string repoDir = TestUtils.ExtractZippedRepo("vcpkg.git");
            RepositoryAnalyzer repositoryAnalyzer = new RepositoryAnalyzer(repoDir);
            CommitDelta changes = repositoryAnalyzer.GetChanges("dbab03a1a82913ae96bfa3c1613ade20b5ac438d");
            Assert.AreEqual(0, changes.Added.Count);
            Assert.AreEqual(0, changes.Deleted.Count);
            Assert.AreEqual(1, changes.Modified.Count);
            FileChange portfileChanges = changes.Modified.Where(x => x.Path == @"ports\openssl\portfile.cmake").First();
            Assert.AreEqual(1, portfileChanges.NumberOfLinesDeleted);
            Assert.AreEqual(0, portfileChanges.NumberOfLinesAdded);
        }


        [Test]
        public void TestGetChangesNewlineAddedToEndOfFile()
        {
            string repoDir = TestUtils.ExtractZippedRepo("html-generator.git");
            RepositoryAnalyzer repositoryAnalyzer = new RepositoryAnalyzer(repoDir);
            CommitDelta changes = repositoryAnalyzer.GetChanges("54a1f0028d595e73ea3151459cb7f9e58ec46b0b");
            FileChange portfileChanges = changes.Modified.Where(x => x.Path == @"src\src\HtmlGenerator\Tag.cs").First();
            Assert.AreEqual(1, portfileChanges.NumberOfLinesDeleted);
            Assert.AreEqual(2, portfileChanges.NumberOfLinesAdded);
        }


        [Test]
        public void TestGetFileLineCounts()
        {
            string repoDir = TestUtils.ExtractZippedRepo("csharplang.git");
            RepositoryAnalyzer repositoryAnalyzer = new RepositoryAnalyzer(repoDir);
            Dictionary<string, int> lineCounts = repositoryAnalyzer.GetFileLineCounts("7981ea1fb4d89571fd41e3b75f7c9f7fc178e837").ToDictionary(x => x.FilePath, x => x.LineCount);
            Assert.AreEqual(6, lineCounts.Count);
            Assert.AreEqual(10, lineCounts.GetValueOrDefault(@"README.md", -1));
            Assert.AreEqual(0, lineCounts.GetValueOrDefault(@"proposals\async-streams.md",-1));
            Assert.AreEqual(126, lineCounts.GetValueOrDefault(@"proposals\nullable-reference-types.md", -1));
            Assert.AreEqual(74, lineCounts.GetValueOrDefault(@"design-notes\Notes-2016-11-16.md", -1));
            Assert.AreEqual(0, lineCounts.GetValueOrDefault(@"design-notes\csharp-language-design-notes-2017.md", -1));
            Assert.AreEqual(0, lineCounts.GetValueOrDefault(@"spec\spec.md", -1));
        }

        [Test]
        public void TestGetCommitsForPath()
        {
            string repoDir = TestUtils.ExtractZippedRepo("csharplang.git");
            RepositoryAnalyzer repositoryAnalyzer = new RepositoryAnalyzer(repoDir);
            IReadOnlyList<Commit> commits = repositoryAnalyzer.GetCommitsForOneFilePath(@"proposals\lambda-attributes.md");
            Assert.AreEqual(2, commits.Count);
            Assert.AreEqual("24b2ac25986949e39fbb258b39c38c5f134afa74", commits[0].Hash);
            Assert.AreEqual("2531e639b6cc3cce1d632daf350f5e49cd000c62", commits[1].Hash);
        }
        
        [Test]
        public void TestGetCommitsForAllFilePaths()
        {
            string repoDir = TestUtils.ExtractZippedRepo("csharplang.git");
            RepositoryAnalyzer repositoryAnalyzer = new RepositoryAnalyzer(repoDir);
            IReadOnlyDictionary<string, IReadOnlyList<Commit>> result = repositoryAnalyzer.GetCommitsForAllFilePaths();
            Assert.AreEqual(152, result.Count);
            Assert.AreEqual(1, result[@"spec\LICENSE.md"].Count);
            Assert.AreEqual("6027ad5a4ab013f4fb42f5edd2d667d649fe1bd8", result[@"spec\LICENSE.md"][0].Hash);
            Assert.AreEqual(4, result[@"proposals\records.md"].Count);
            Assert.AreEqual("658e65403a9e14d9f614994d1fd6757d4fab9608", result[@"proposals\records.md"][0].Hash);
            Assert.AreEqual("22f42429d2b5d391db632ef0cc2444844118f108", result[@"proposals\records.md"][1].Hash);
            Assert.AreEqual("9d957f97b19645397c97340cd996e73b952b30b1", result[@"proposals\records.md"][2].Hash);
            Assert.AreEqual("fab2157f7e13c8f12a781172d9b870f846a442b9", result[@"proposals\records.md"][3].Hash);
        }
    }
}