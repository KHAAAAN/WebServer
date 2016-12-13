using System;
using NUnit.Framework;
using System.IO;
using System.Collections.Generic;

namespace CS422
{
    [TestFixture]
    public class StdFSTest
    {
        StandardFileSystem stdFS;
        Dir422 root;

        //change this if you want to test somewhere on your own filesystem. 
        const string rootString = "/home/jay/422/HW8Test"; 

        [SetUp] //like @Before
        public void Init(){ //create standard file system
            Assert.True(Directory.Exists(rootString)); //make sure this directory exists.

            stdFS = StandardFileSystem.Create(rootString);
            root = stdFS.GetRoot();

            Dir422 one = root.CreateDir("one");
            Dir422 two = root.CreateDir("two");

            Dir422 curr1 = one;
            Dir422 curr2 = two;

            // /one/a/b/c and /two/a/b/c
            for (int i = 0; i < 3; i++)
            {
                curr1 = curr1.CreateDir(((char)('a' + i)).ToString());
                curr2 = curr2.CreateDir(((char)('a' + i)).ToString());
            }
        }

        [TearDown]
        public void CleanUp(){
            var dir = new DirectoryInfo(rootString);
            foreach (FileInfo file in dir.GetFiles())
            {
                file.Delete(); 
            }
            foreach (DirectoryInfo d in dir.GetDirectories())
            {
                d.Delete(true); 
            }
        }

        [Test]
        public void StandardFileSystemGetRootTest(){
            Assert.NotNull(stdFS.GetRoot());
        }

        [Test]
        public void RootParentIsNullTest(){
            Assert.Null(root.Parent);
        }

        [Test]
        public void StandardFileSystemContainsDirTest(){
            Dir422 c = root.GetDir("two").GetDir("a").GetDir("b").GetDir("c");

            Assert.True(stdFS.Contains(c));
        }

        [Test]
        public void StandardFileSystemContainsFileTest(){
            Dir422 c = root.GetDir("two").GetDir("a").GetDir("b").GetDir("c");
            File422 txt = c.CreateFile("dummy.txt");

            Assert.True(stdFS.Contains(txt));
        }

        [Test]
        public void StdFSDirCreateDirTest(){
            Dir422 three = root.CreateDir("three");
            Assert.AreEqual("three", three.Name);
        }

        [Test]
        public void StdFSDirCreateDirDupeTest(){

            Dir422 three = root.CreateDir("three");
            Dir422 threeDupe = root.CreateDir("three"); //duplicate, since we already created dir

            //since it's not in memory, we don't have reference
            //to same object, so just compare names.
            Assert.AreEqual(three.Name, threeDupe.Name);
        }

        [Test]
        public void StdFSDirCreateFileTest(){
            Dir422 b = root.GetDir("one").GetDir("a").GetDir("b");
            File422 bFile = b.CreateFile("bFile.txt");

            Assert.AreEqual("bFile.txt", bFile.Name);
            //references are different. Since not memory. we create new everytime.
        }

        [Test]
        public void StdFSDirCreateFileDupeTest(){

            File422 txt = root.CreateFile("file.txt");
            File422 txtDupe = root.CreateFile("file.txt");

            Assert.AreEqual(txt.Name, txtDupe.Name);

        }

        [Test]
        public void StdFSDirNameTest(){
            Dir422 a = root.GetDir("one").GetDir("a");
            Assert.AreEqual("a", a.Name);
        }

        [Test]
        public void StdFSDirParentTest(){
            Dir422 two = root.GetDir("one");
            Dir422 a = two.GetDir("a");

            Assert.AreSame(two, a.Parent);
        }

        [Test]
        public void StdFSDirGetDirNotNullTest(){
            Dir422 two = root.GetDir("two");
            Assert.NotNull(two);
        }

        [Test]
        public void StdFSDirGetDirNullTest(){
            Dir422 three = root.GetDir("three");
            Assert.Null(three);
        }

        [Test]
        public void StdFSDirGetFileNotNullTest(){
            root.CreateFile("root.txt");
            File422 file = root.GetFile("root.txt");
            Assert.NotNull(file);
        }

        [Test]
        public void StdFSDirGetFileNullTest(){
            File422 file = root.GetFile("root.txt");
            Assert.Null(file);
        }

        [Test]
        public void StdFSDirGetDirsTest(){
            IList<Dir422> dirs = root.GetDirs();

            Assert.AreEqual(2, dirs.Count);
        }

        [Test]
        public void StdFSDirGetFilesTest(){
            Dir422 b = root.GetDir("two").GetDir("a").GetDir("b");
            b.CreateFile("file1.txt");
            b.CreateFile("file2.txt");

            IList<File422> files = b.GetFiles();

            Assert.AreEqual(2, files.Count);
        }

        [Test]
        public void StdFSDirContainsDirNonRecursiveTest(){
            Assert.True(root.ContainsDir("one", false));
            Assert.False(root.ContainsDir("c", false));
        }

        [Test]
        public void StdFSDirContainsDirRecursiveTest(){
            Assert.True(root.ContainsDir("one", true));
            Assert.True(root.ContainsDir("c", true));
        }

        [Test]
        public void StdFSDirContainsFileNonRecursiveTest(){
            Dir422 c = root.GetDir("one").GetDir("a").GetDir("b").GetDir("c");
            root.CreateFile("at_root.txt");
            c.CreateFile("at_ones_c.txt");

            Assert.True(root.ContainsFile("at_root.txt", false));
            Assert.False(root.ContainsFile("at_ones_c.txt", false));
        }

        [Test]
        public void StdFSDirContainsFileRecursiveTest(){
            Dir422 c = root.GetDir("one").GetDir("a").GetDir("b").GetDir("c");
            root.CreateFile("at_root.txt");
            c.CreateFile("at_ones_c.txt");

            Assert.True(root.ContainsFile("at_root.txt", true));
            Assert.True(root.ContainsFile("at_ones_c.txt", true));
        }

        [Test]
        public void StdFSFileNameTest(){
            File422 file = root.CreateFile("file.txt");
            Assert.AreEqual("file.txt", file.Name); 
        }

        [Test]
        public void StdFSFileParentTest(){
            File422 file = root.CreateFile("file.txt");
            Assert.AreSame(root, file.Parent);
        }

        [Test]
        public void StdFSFileReadOnlySharedAccessTest(){
            
            File422 file = root.CreateFile("file.txt");

            Stream stream1 = file.OpenReadOnly();
            Stream stream2 = file.OpenReadOnly();
            Stream stream3 = file.OpenReadWrite();

            Assert.NotNull(stream1);
            Assert.NotNull(stream2);
            Assert.Null(stream3);

        }

        [Test]
        public void StdFSFileReadWriteSharedAccessTest(){
            File422 file = root.CreateFile("file.txt");

            Stream stream3 = file.OpenReadWrite();
            Stream stream1 = file.OpenReadOnly();
            Stream stream2 = file.OpenReadOnly();

            Assert.NotNull(stream3);
            Assert.Null(stream1);
            Assert.Null(stream2);
        }
    }
}
