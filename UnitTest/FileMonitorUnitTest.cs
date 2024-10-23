/******************************************************************************
* Filename    = FileMonitorUnitTest.cs
*
* Author      = Karumudi Harika
*
* Product     = Updater.Client
* 
* Project     = Unit Test
*
* Description = Unit Test for ViewModel.
*****************************************************************************/

using Microsoft.VisualStudio.TestTools.UnitTesting;
using FileWatcherMVVM1.ViewModel;
using System.IO;
using System.Threading;

namespace UnitTest
{
    /// <summary>
    /// Unit test class for FileMonitor. This class contains tests that simulate file events 
    /// (creation, deletion) and verify if the FileMonitor's MessageStatus is updated correctly.
    /// </summary>
    [TestClass]
    public class FileMonitorUnitTest
    {
        private FileMonitor _fileMonitor;

        /// <summary>
        /// Setup method to initialize the FileMonitor instance before each test.
        /// </summary>
        [TestInitialize]
        public void Setup()
        {
            // Initialize the FileMonitor before every test
            _fileMonitor = new FileMonitor();
        }

        /// <summary>
        /// Test method to simulate file creation and verify that the MessageStatus reflects the 
        /// file creation event correctly.
        /// </summary>
        [TestMethod]
        public void TestFileCreatedUpdateMessageStatus()
        {
            //Define a test file path that will simulate the file creation
            string testFilePath = @"C:\Users\harik\Downloads\testfile.txt";

            // Act: Simulate file creation event using reflection to call private method
            _fileMonitor.GetType()
                .GetMethod("OnFileCreated", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.Invoke(_fileMonitor, new object[] { this, new FileSystemEventArgs(WatcherChangeTypes.Created, Path.GetDirectoryName(testFilePath), Path.GetFileName(testFilePath)) });

            // Simulate timer elapse
            Thread.Sleep(1100);

            // Check if the MessageStatus reflects the correct message
            Assert.AreEqual("Files created: testfile.txt", _fileMonitor.MessageStatus.TrimEnd());
        }

        /// <summary>
        /// Test method to simulate file deletion and verify that the MessageStatus reflects the 
        /// file deletion event correctly.
        /// </summary>
        [TestMethod]
        public void TestFileDeletedUpdateMessageStatus()
        {
            //Define a test file path for deletion
            string testFilePath = @"C:\Users\harik\Downloads\testfile.txt";

            //Simulate file deletion event using reflection
            _fileMonitor.GetType()
                .GetMethod("OnFileDeleted", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.Invoke(_fileMonitor, new object[] { this, new FileSystemEventArgs(WatcherChangeTypes.Deleted, Path.GetDirectoryName(testFilePath), Path.GetFileName(testFilePath)) });

            // Simulate timer elapse
            Thread.Sleep(1100);

            //Ensure the MessageStatus correctly updates the deletion message
            Assert.AreEqual("Files removed: testfile.txt", _fileMonitor.MessageStatus.TrimEnd());
        }

        /// <summary>
        /// Test method to simulate multiple file creation and deletion events and verify that 
        /// the MessageStatus reflects all file changes correctly.
        /// </summary>
        [TestMethod]
        public void TestMultipleFilesUpdateMessageStatus()
        {
            //Define multiple file paths for testing multiple events
            string file1 = @"C:\Users\harik\Downloads\file1.txt";
            string file2 = @"C:\Users\harik\Downloads\file2.txt";
            string deletedFile = @"C:\Users\harik\Downloads\deletedfile.txt";

            //Simulate multiple file events using reflection
            _fileMonitor.GetType()
                .GetMethod("OnFileCreated", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.Invoke(_fileMonitor, new object[] { this, new FileSystemEventArgs(WatcherChangeTypes.Created, Path.GetDirectoryName(file1), Path.GetFileName(file1)) });

            _fileMonitor.GetType()
                .GetMethod("OnFileCreated", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.Invoke(_fileMonitor, new object[] { this, new FileSystemEventArgs(WatcherChangeTypes.Created, Path.GetDirectoryName(file2), Path.GetFileName(file2)) });

            _fileMonitor.GetType()
                .GetMethod("OnFileDeleted", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.Invoke(_fileMonitor, new object[] { this, new FileSystemEventArgs(WatcherChangeTypes.Deleted, Path.GetDirectoryName(deletedFile), Path.GetFileName(deletedFile)) });

            // Simulate timer elapse
            Thread.Sleep(1100);

            //Ensure that the MessageStatus is correctly updated for multiple files
            Assert.AreEqual(
                "Files created: file1.txt, file2.txt\nFiles removed: deletedfile.txt".Replace("\r\n", "\n").Trim(),
                _fileMonitor.MessageStatus.Replace("\r\n", "\n").Trim()
            );



        }
    }
}

