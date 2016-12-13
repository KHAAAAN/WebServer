using System;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace CS422
{
    [TestFixture]
    public class MemFSTest
    {

        MemoryFileSystem memFS;
        Dir422 root;

        [SetUp] //like @Before
        public void Init(){ //create memory file system
            memFS = new MemoryFileSystem();
            root = memFS.GetRoot();

            //do different subdirectory routes
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

        [Test]
        public void MemoryFileSystemGetRootTest(){
            Assert.True(String.IsNullOrEmpty(memFS.GetRoot().Name));
        }

        [Test]
        public void RootParentIsNullTest(){
            Assert.Null(root.Parent);
        }

        [Test]
        public void MemoryFileSystemContainsDirTest(){
            Dir422 c = root.GetDir("two").GetDir("a").GetDir("b").GetDir("c");

            Assert.True(memFS.Contains(c));
        }

        [Test]
        public void MemoryFileSystemContainsFileTest(){
            Dir422 c = root.GetDir("two").GetDir("a").GetDir("b").GetDir("c");
            File422 txt = c.CreateFile("dummy.txt");

            Assert.True(memFS.Contains(txt));
        }

        [Test]
        public void MemFSDirCreateDirTest(){
            Dir422 three = root.CreateDir("three");
            Assert.AreEqual("three", three.Name);
        }

        [Test]
        public void MemFSDirCreateDirDupeTest(){
            
            Dir422 three = root.CreateDir("three");
            Dir422 threeDupe = root.CreateDir("three"); //duplicate, since we already created dir
            Assert.AreSame(three, threeDupe);
        }

        [Test]
        public void MemFSDirCreateFileTest(){

            Dir422 b = root.GetDir("one").GetDir("a").GetDir("b");
            File422 bFile = b.CreateFile("bFile.txt");

            Assert.AreEqual("bFile.txt", bFile.Name);
            Assert.AreSame(b, bFile.Parent);
        }

        [Test]
        public void MemFSDirCreateFileDupeTest(){

            File422 txt = root.CreateFile("file.txt");
            File422 txtDupe = root.CreateFile("file.txt");

            Assert.AreSame(txt, txtDupe);

        }

        [Test]
        public void MemFSDirNameTest(){
            Dir422 a = root.GetDir("one").GetDir("a");
            Assert.AreEqual("a", a.Name);
        }

        [Test]
        public void MemFSDirParentTest(){
            Dir422 two = root.GetDir("one");
            Dir422 a = two.GetDir("a");

            Assert.AreSame(two, a.Parent);
        }

        [Test]
        public void MemFSDirGetDirNotNullTest(){
            Dir422 two = root.GetDir("two");
            Assert.NotNull(two);
        }

        [Test]
        public void MemFSDirGetDirNullTest(){
            Dir422 three = root.GetDir("three");
            Assert.Null(three);
        }

        [Test]
        public void MemFSDirGetFileNotNullTest(){
            root.CreateFile("root.txt");
            File422 file = root.GetFile("root.txt");
            Assert.NotNull(file);
        }

        [Test]
        public void MemFSDirGetFileNullTest(){
            File422 file = root.GetFile("root.txt");
            Assert.Null(file);
        }

        [Test]
        public void MemFSDirGetDirsTest(){
            IList<Dir422> dirs = root.GetDirs();
            Dir422 one = root.GetDir("one");
            Dir422 two = root.GetDir("two");

            Assert.AreEqual(2, dirs.Count);
            Assert.AreSame(one, dirs[dirs.IndexOf(one)]);
            Assert.AreSame(two, dirs[dirs.IndexOf(two)]);
        }

        [Test]
        public void MemFSDirGetFilesTest(){
            Dir422 b = root.GetDir("two").GetDir("a").GetDir("b");
            File422 file1 = b.CreateFile("file1.txt");
            File422 file2 = b.CreateFile("file2.txt");

            IList<File422> files = b.GetFiles();

            Assert.AreEqual(2, files.Count);
            Assert.AreSame(file1, files[files.IndexOf(file1)]);
            Assert.AreSame(file2, files[files.IndexOf(file2)]);
        }

        [Test]
        public void MemFSDirContainsDirNonRecursiveTest(){
            Assert.True(root.ContainsDir("one", false));
            Assert.False(root.ContainsDir("c", false));
        }

        [Test]
        public void MemFSDirContainsDirRecursiveTest(){
            Assert.True(root.ContainsDir("one", true));
            Assert.True(root.ContainsDir("c", true));
        }

        [Test]
        public void MemFSDirContainsFileNonRecursiveTest(){
            Dir422 c = root.GetDir("one").GetDir("a").GetDir("b").GetDir("c");
            root.CreateFile("at_root.txt");
            c.CreateFile("at_ones_c.txt");

            Assert.True(root.ContainsFile("at_root.txt", false));
            Assert.False(root.ContainsFile("at_ones_c.txt", false));
        }

        [Test]
        public void MemFSDirContainsFileRecursiveTest(){
            Dir422 c = root.GetDir("one").GetDir("a").GetDir("b").GetDir("c");
            root.CreateFile("at_root.txt");
            c.CreateFile("at_ones_c.txt");

            Assert.True(root.ContainsFile("at_root.txt", true));
            Assert.True(root.ContainsFile("at_ones_c.txt", true));
        }


        [Test]
        public void MemFSFileNameTest(){
            File422 file = root.CreateFile("file.txt");
            Assert.AreEqual("file.txt", file.Name); 
        }

        [Test]
        public void MemFSFileParentTest(){
            File422 file = root.CreateFile("file.txt");
            Assert.AreSame(root, file.Parent);
        }

        [Test]
        public void MemFSFileReadOnlySharedAccessTest(){
            File422 file = root.CreateFile("file.txt");

            Stream stream1 = file.OpenReadOnly();
            Stream stream2 = file.OpenReadOnly();
            Stream stream3 = file.OpenReadWrite();

            Assert.NotNull(stream1);
            Assert.NotNull(stream2);
            Assert.Null(stream3);

            Assert.AreEqual(2, ((MemFSFile)file).RefCount);

            stream1.Dispose();

            Assert.AreEqual(1, ((MemFSFile)file).RefCount);

            stream2.Dispose();

            Assert.AreEqual(0, ((MemFSFile)file).RefCount);

        }

        [Test]
        public void MemFSFileReadWriteSharedAccessTest(){
            File422 file = root.CreateFile("file.txt");

            Stream stream3 = file.OpenReadWrite();
            Stream stream1 = file.OpenReadOnly();
            Stream stream2 = file.OpenReadOnly();

            Assert.NotNull(stream3);
            Assert.Null(stream1);
            Assert.Null(stream2);


            Assert.AreEqual(1, ((MemFSFile)file).RefCount);

            stream3.Dispose();

            Assert.AreEqual(0, ((MemFSFile)file).RefCount);

        }

    }
}

