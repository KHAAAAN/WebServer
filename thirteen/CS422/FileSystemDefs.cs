using System;
using System.Collections.Generic;
using System.IO;

namespace CS422
{
    public abstract class Dir422{
        
        public abstract string Name { get;}

        public abstract IList<Dir422>GetDirs();

        public abstract IList<File422>GetFiles();

        public abstract Dir422 Parent {get;}


        public abstract bool ContainsFile(string fileName, bool recursive);

        public abstract bool ContainsDir(string dirName, bool recursive);


        public abstract Dir422 GetDir(string dirName);

        public abstract File422 GetFile(string fileName);


        public abstract Dir422 CreateDir(string fileName);

        public abstract File422 CreateFile(string fileName);

    }

    public abstract class File422{
        
        public abstract string Name {get;}

        public abstract Stream OpenReadOnly();

        public abstract Stream OpenReadWrite();

        public abstract Dir422 Parent {get;}
    }

    public abstract class FileSys422{

        public abstract Dir422 GetRoot();

        //made this virtual so we dont have to override
        public virtual bool Contains(Dir422 directory){

            bool contains = false;

            Dir422 root = GetRoot();
            Dir422 cur = directory;

            //cur traverses up the parent ladder to
            //see if it eventually matchess root's name
            while (cur != null)
            {
                if (cur.Name == root.Name)
                {
                    contains = true;
                    break;
                }

                cur = cur.Parent;
            }

            return contains;
        }

        //this is also virtual but with file instead of directory i.e
        //method overloading
        public virtual bool Contains(File422 file){

            bool contains = false;

            Dir422 root = GetRoot();
            Dir422 cur = file.Parent; //start at parent.

            while (cur != null)
            {
                if (cur.Name == root.Name)
                {
                    contains = true;
                    break;
                }

                cur = cur.Parent;
            }

            return contains;
        }
    }

    //this is just a helper class for string functions that we need.
    internal class StdFSHelper{
        
        internal static string getBaseFromPath(string path){
            string[] split = path.Split(new char[]{'/'}, StringSplitOptions.RemoveEmptyEntries);

            string s = "";

            if (split.Length > 0) //incase it is at actual root "/"
            {
                s = split[split.Length - 1];
            }

            return s;
        }

        //name can be dir or file name.
        internal static bool ContainsPathCharacters(string name){
            return name.Contains("/") || name.Contains(@"\");
        }
    }

    public class StdFSDir : Dir422{

        private string _name;
        private Dir422 _parent;
        private string _path;

        public StdFSDir(string path, Dir422 parent = null){
            _path = path;
            _parent = parent;

            if (parent != null)
            {
                _name = StdFSHelper.getBaseFromPath(path);
            }
            else
            {
                _name = path;
            }
        }

        public override string Name
        {
            get
            {
                return _name;
            }
        }

        public override Dir422 Parent
        {
            get
            {
                return _parent;
            }
        }

        public override IList<Dir422> GetDirs()
        {
            string[] pathNames = Directory.GetDirectories(_path);
            IList<Dir422> dirs = new List<Dir422>();

            foreach (string pathName in pathNames)
            {
                dirs.Add(new StdFSDir(pathName, this));
            }

            return dirs;
        }

        public override IList<File422> GetFiles()
        {
            string[] pathNames = Directory.GetFiles(_path);
            IList<File422> files = new List<File422>();

            foreach (string pathName in pathNames)
            {
                files.Add(new StdFSFile(pathName, this));
            }

            return files;
        }

        public override bool ContainsFile(string fileName, bool recursive)
        {
            bool containsFile = false;

            //return false immedietly if it contains those characters
            if (!StdFSHelper.ContainsPathCharacters(fileName))
            {
                if (recursive)
                {
                    containsFile = ContainsFileRecursive(fileName);
                }
                else
                {
                    //name is in format /a/b/c/d where d is base of the path
                    IList<File422> files = GetFiles();

                    foreach (File422 file in files)
                    {
                        //so this gets 'd'
                        if (file.Name == fileName)
                        {
                            containsFile = true;
                            break;
                        }
                    }
                }
            }

            return containsFile;
        }

        private bool ContainsFileRecursive(string fileName){

            bool containsFile = ContainsFile(fileName, false);

            if (containsFile)
            {
                return true;
            }

            IList<Dir422> dirs = GetDirs();
            if (dirs.Count == 0) //no more directories exist to search
            {
                return false;
            }

            //else
            foreach (Dir422 dir in dirs)
            {
                //recursively check if subdirs have file
                if(dir.ContainsFile(fileName, true)){
                    containsFile = true;
                    break;
                }
            }

            return containsFile;
        }

        public override bool ContainsDir(string dirName, bool recursive)
        {
            bool containsDir = false;

            if (!StdFSHelper.ContainsPathCharacters(dirName))
            {
                if (recursive)
                {
                    containsDir = ContainsDirRecursive(dirName);
                }
                else
                {
                    //name is in format /a/b/c/d where d is base of the path
                    IList<Dir422> dirs = GetDirs();

                    foreach (Dir422 dir in dirs)
                    {
                        if (dir.Name == dirName)
                        {
                            containsDir = true;
                            break;
                        }
                    }
                }
            }

            return containsDir;
        }

        private bool ContainsDirRecursive(string dirName){

            bool containsDir = ContainsDir(dirName, false);

            if (containsDir)
            {
                return true;
            }

            IList<Dir422> dirs = GetDirs();
            if (dirs.Count == 0)
            {
                return false;
            }

            //else
            foreach (Dir422 dir in dirs)
            {
                //recursively check if subdirs have file
                if(dir.ContainsDir(dirName, true)){
                    containsDir = true;
                    break;
                }
            }

            return containsDir;
        }

        public override Dir422 GetDir(string dirName){

            Dir422 stdDir = null;
            IList<Dir422> dirs = GetDirs();

            if (dirs.Count != 0 && !StdFSHelper.ContainsPathCharacters(dirName))
            {
                foreach (Dir422 dir in dirs)
                {
                    if (dir.Name == dirName)
                    {
                        stdDir = dir;
                        break;
                    }
                }
            }

            return stdDir;
        }

        public override File422 GetFile(string fileName){
            File422 stdFile = null;
            IList<File422> files = GetFiles();

            if (files.Count != 0 && !StdFSHelper.ContainsPathCharacters(fileName))
            {
                foreach (File422 file in files)
                {
                    if (file.Name == fileName)
                    {
                        stdFile = file;
                        break;
                    }
                }
            }

            return stdFile;
        }

        public override File422 CreateFile(string fileName)
        {
            File422 stdFile = null;

            //if at root of filesystem, gets rid of extra '/'
            string path = (_path == "/") ? "" : _path;

            if (!StdFSHelper.ContainsPathCharacters(fileName) 
                || string.IsNullOrEmpty(fileName)){

                //if we are at /a/b/, makes /a/b/c.txt
                //where c.txt is fileName
                path = path + "/" + fileName;

                //if already exists clear existing data.
                if(File.Exists(path)){
                    File.WriteAllText(path, "");
                }
                    
                //if exists and not read-only contents are overwritten
                using (FileStream fs = File.Create(path))
                {
                    //set length to 0.
                    fs.SetLength(0);

                    //this is the parent
                    stdFile = new StdFSFile(path, this);
                }
            }

            return stdFile;
        }

        public override Dir422 CreateDir(string fileName)
        {
            Dir422 stdDir = null;

            //if at root of filesystem, gets rid of extra '/'
            string path = (_path == "/") ? "" : _path;

            if (!StdFSHelper.ContainsPathCharacters(fileName)
                || string.IsNullOrEmpty(fileName))
            {
                //if we are at /a/b/, makes /a/b/c
                //where c is fileName
                path = path + "/" + fileName;

                Directory.CreateDirectory(path);

                //this is the parent
                stdDir = new StdFSDir(path, this);
            }

            return stdDir;
        }
    }
   
    public class StdFSFile : File422{

        private string _name;
        private Dir422 _parent;
        private string _path;

        //parent cannot be null
        public StdFSFile(string path, Dir422 parent){
            _path = path;
            _parent = parent;

            _name = StdFSHelper.getBaseFromPath(path);
        }

        public override string Name{
            get{ 
                return _name; 
            }
        }

        public override Dir422 Parent
        {
            get
            {
                return _parent;
            }
        }

        public override Stream OpenReadOnly()
        {
            Stream stream = null;

            try{
            stream = new FileStream(_path, FileMode.Open,
                FileAccess.Read);
            } catch(Exception){

            }

            return stream;
        }

        public override Stream OpenReadWrite()
        {
            Stream stream = null;

            try{
            stream = new FileStream(_path, FileMode.Open,
                                FileAccess.ReadWrite);
            } catch(Exception){

            }

            return stream;
        }
    }

    public class StandardFileSystem : FileSys422{

        private Dir422 _root;

        public static StandardFileSystem Create(string path){
            StandardFileSystem sfs = null;


            if (Directory.Exists(path))
            {
                sfs = new StandardFileSystem(path); // /a/b/c/d -> parent = null
            }

            return sfs;
        }

        public StandardFileSystem(string path){
            _root = new StdFSDir(path);
        }

        public override Dir422 GetRoot()
        {
            return _root;
        }
    }

    public class MemFSDir : Dir422{
        private string _name;
        private Dir422 _parent;

        IList<Dir422> _childDirs;
        IList<File422> _childFiles;

        public MemFSDir(string name, Dir422 parent = null){
            _parent = parent;
            _name = name;

            Initialize();
        }

        private void Initialize(){
            _childDirs = new List<Dir422>();
            _childFiles = new List<File422>();
        }

        public override string Name
        {
            get
            {
                return _name;
            }
        }

        public override Dir422 Parent
        {
            get
            {
                return _parent;
            }
        }

        public override IList<Dir422> GetDirs()
        {
            return _childDirs;
        }

        public override IList<File422> GetFiles()
        {
            return _childFiles;
        }

        public override bool ContainsFile(string fileName, bool recursive)
        {
            bool containsFile = false;

            //return false immedietly if it contains those characters
            if (!StdFSHelper.ContainsPathCharacters(fileName))
            {
                if (recursive)
                {
                    containsFile = ContainsFileRecursive(fileName);
                }
                else
                {
                    foreach (File422 childFile in _childFiles)
                    {
                        if (childFile.Name == fileName)
                        {
                            containsFile = true;
                            break;
                        }
                    }
                }
            }

            return containsFile;
        }

        private bool ContainsFileRecursive(string fileName){

            bool containsFile = ContainsFile(fileName, false);

            if (containsFile)
            {
                return true;
            }

            IList<Dir422> dirs = GetDirs();
            if (dirs.Count == 0) //no more directories exist to search
            {
                return false;
            }

            //else
            foreach (Dir422 dir in dirs)
            {
                //recursively check if subdirs have file
                if(dir.ContainsFile(fileName, true)){
                    containsFile = true;
                    break;
                }
            }

            return containsFile;
        }

        public override bool ContainsDir(string dirName, bool recursive)
        {
            bool containsDir = false;

            if (!StdFSHelper.ContainsPathCharacters(dirName))
            {
                if (recursive)
                {
                    containsDir = ContainsDirRecursive(dirName);
                }
                else
                {
                    foreach (Dir422 childDir in _childDirs)
                    {
                        if (childDir.Name == dirName)
                        {
                            containsDir = true;
                            break;
                        }
                    }
                }
            }

            return containsDir;
        }

        private bool ContainsDirRecursive(string dirName){

            bool containsDir = ContainsDir(dirName, false);

            if (containsDir)
            {
                return true;
            }

            IList<Dir422> dirs = GetDirs();
            if (dirs.Count == 0)
            {
                return false;
            }

            //else
            foreach (Dir422 dir in dirs)
            {
                //recursively check if subdirs have file
                if(dir.ContainsDir(dirName, true)){
                    containsDir = true;
                    break;
                }
            }

            return containsDir;
        }

        public override Dir422 GetDir(string dirName)
        {
            Dir422 dir = null;

            if (!StdFSHelper.ContainsPathCharacters(dirName))
            {
                foreach(Dir422 childDir in _childDirs){

                    if (childDir.Name == dirName)
                    {
                        dir = childDir;
                        break;
                    }
                }
            }

            return dir;
        }

        public override File422 GetFile(string fileName)
        {
            File422 file = null;

            if (!StdFSHelper.ContainsPathCharacters(fileName))
            {
                foreach (File422 childFile in _childFiles)
                {
                    if (childFile.Name == fileName)
                    {
                        file = childFile;
                        break;
                    }
                }
            }

            return file;
        }

        public override File422 CreateFile(string fileName)
        {
            File422 memFile = null;

            if (!StdFSHelper.ContainsPathCharacters(fileName) 
                || string.IsNullOrEmpty(fileName)){

                //check if exists already
                if ((memFile = GetFile(fileName)) != null)
                {
                    using (Stream s = memFile.OpenReadWrite())
                    {

                        if (s != null)
                        {
                            //this 0's out the memory stream.
                            s.SetLength(0);
                        }
                    }
                }
                else
                {
                    memFile = new MemFSFile(fileName, this);
                    _childFiles.Add(memFile);
                }
            }

            return memFile;
        }

        public override Dir422 CreateDir(string fileName)
        {
            Dir422 memDir = null;

            if (!StdFSHelper.ContainsPathCharacters(fileName)
                || string.IsNullOrEmpty(fileName))
            {
                //check if already exists
                if ((memDir = GetDir(fileName)) == null)
                {
                    memDir = new MemFSDir(fileName, this);
                    _childDirs.Add(memDir);
                }
            }

            return memDir;
        }
    }

    public class MemFSFile : File422{

        private string _name;
        private Dir422 _parent;
        private byte[] _memFileBuffer; //our file imitiation
        private const int SIZE = Int32.MaxValue;

        public MemFSFile(string name, Dir422 parent){
            _name = name;
            _parent = parent;

            Initialize();
        }

        private void Initialize(){
            _memFileBuffer = new byte[SIZE];
            _refCount = 0;
            _isReadWrite = false;
            _lock = new object();
        }

        public override string Name
        {
            get
            {
                return _name;
            }
        }

        public override Dir422 Parent
        {
            get
            {
                return _parent;
            }
        }

        private int _refCount;
        public int RefCount{get{return _refCount;}} 

        private bool _isReadWrite;
        private object _lock;

        /* NOTE:
         * 
         * The locking mechanism works here,
         * since all we are making sure is
         * each thread calling these individual methods
         * have the most up to date values of _isReadWrite
         * and in the case of OpenReadWrite, the most
         * updated version of refCount
         */
        public override Stream OpenReadOnly()
        {
            MemoryFileStream mfs = null;

            //all reading threads synchronize here
            lock (_lock)
            {
                if (!_isReadWrite)
                {
                    
                    _refCount += 1;
                    mfs = new MemoryFileStream(_memFileBuffer, false, () =>
                        {
                            /* 
                             * It doesn't matter if
                             * _refCount decrements here,
                             * all reading threads will still
                             * be able to access this if statement
                             * code block inside of the lock.
                             * 
                             * However, if we decrement
                             * and it goes to 0 and a thread has a lock
                             * inside OpenReadWrite(), and its just 
                             * about to check the if statement, it will pass
                             * however if that very thread is locked
                             * and we have not decremented our refcount
                             * yet, it wont go inside that if statement,
                             * so either way we are thread-safe
                             */
                            _refCount -= 1;
                        });

                }
            }

            return mfs;
        }

        public override Stream OpenReadWrite()
        {
            MemoryFileStream mfs = null;

            /* Writing threads will synchronize with reading threads, in order
             * of when they were called. This way refCount and ReadWrite
             * are unchanged values once we enter either block of code. (in OpenReadOnly
             * or here)
            */
            lock (_lock)
            {
                if (!_isReadWrite && _refCount == 0)
                {
                    _isReadWrite = true;
                    mfs = new MemoryFileStream(_memFileBuffer, true, () =>
                        {
                            _refCount -= 1;
                            _isReadWrite = false;
                        });

                    _refCount += 1;
                }
            }

            return mfs;
        }
    }

    public class MemoryFileSystem : FileSys422 {

        Dir422 _root;

        public MemoryFileSystem(){
            _root = new MemFSDir("");
        }

        public override Dir422 GetRoot()
        {
            return _root;
        }

    }
        
    internal class MemoryFileStream : MemoryStream{

        private Action _callback;
        public delegate void Action();

        public MemoryFileStream(byte[] buffer, bool writable,
            Action callback) : base(buffer, writable) {

            _callback = callback;
        }


        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            _callback();
        }
    }
}