HDF5-CSharp helps in reading and writing hdf5 files for .net environments

## Usage

### Input sanitation on filenames
The filename parameter to the methods ```OpenFile(string filename, bool readonly)``` and ```CreateFile(string filename)``` are now input sanitized. This means that you should encapsulate these methods in a try-catch block to ensure any errors are caught.

```csharp
    var filename = @"C:\temp\file.h5"; // will not throw exception
    var anotherFilename = @"c:\temp\זרו.h5"; // will throw exception

    long fileId = 0L;

    try
    {
        fileId = Hdf5.OpenFile(filename);
        fileid = Hdf5.OpenFile(anotherFilename); // throws an exception
    }
    catch(ArgumentOutOfRangeException ex)
    {
        // This is what is thrown when you have illegal charactes in
        // the filename
    }
    catch(ArgumentNullException ex)
    {
        // This is what is thrown if you pass in a null or whitespace
        // string as filename
    }
```

Note that you can catch the ```ArugmentException``` base class if you don't care about distinguishing between these two error cases.

```csharp
    var filename = @"C:\temp\file.h5"; // will not throw exception
    var anotherFilename = @"c:\temp\זרו.h5"; // will throw exception

    long fileId = 0L;

    try
    {
        fileId = Hdf5.OpenFile(filename);
        fileid = Hdf5.OpenFile(anotherFilename); // throws an exception
    }
    catch(ArgumentException ex)
    {
        // This also works. You will not be able to distinguish between 
        // which error this is unless you inspect the message property of
        // the exception.
    }
```

### write an object to an HDF5 file
In the example below an object is created with some arrays and other variables
The object is written to a file and then read back in a new object.


```csharp

     private class TestClassWithArray
        {
            public double[] TestDoubles { get; set; }
            public string[] TestStrings { get; set; }
            public int TestInteger { get; set; }
            public double TestDouble { get; set; }
            public bool TestBoolean { get; set; }
            public string TestString { get; set; }
        }
     var testClass = new TestClassWithArray() {
                    TestInteger = 2,
                    TestDouble = 1.1,
                    TestBoolean = true,
                    TestString = "test string",
                    TestDoubles = new double[] { 1.1, 1.2, -1.1, -1.2 },
                    TestStrings = new string[] { "one", "two", "three", "four" }
        };
    int fileId = Hdf5.CreateFile("testFile.H5");

    Hdf5.WriteObject(fileId, testClass, "testObject");

    TestClassWithArray readObject = new TestClassWithArray();

    readObject = Hdf5.ReadObject(fileId, readObject, "testObject");

    Hdf5.CloseFile(fileId);

```


## Write a dataset and append new data to it

```csharp

    /// <summary>
    /// create a matrix and fill it with numbers
    /// </summary>
    /// <param name="offset"></param>
    /// <returns>the matrix </returns>
    private static double[,]createDataset(int offset = 0)
    {
      var dset = new double[10, 5];
      for (var i = 0; i < 10; i++)
        for (var j = 0; j < 5; j++)
        {
          double x = i + j * 5 + offset;
          dset[i, j] = (j == 0) ? x : x / 10;
        }
      return dset;
    }

    // create a list of matrices
    dsets = new List<double[,]> {
                createDataset(),
                createDataset(10),
                createDataset(20) };

    string filename = Path.Combine(folder, "testChunks.H5");
    int fileId = Hdf5.CreateFile(filename);    

    // create a dataset and append two more datasets to it
    using (var chunkedDset = new ChunkedDataset<double>("/test", fileId, dsets.First()))
    {
      foreach (var ds in dsets.Skip(1))
        chunkedDset.AppendDataset(ds);
    }

    // read rows 9 to 22 of the dataset
    ulong begIndex = 8;
    ulong endIndex = 21;
    var dset = Hdf5.ReadDataset<double>(fileId, "/test", begIndex, endIndex);
    Hdf5.CloseFile(fileId);

```

for more example see unit test project

## Reading H5 File:
you can use the following two method to read the structure of an existing file:

```csharp

string fileName = @"FileStructure.h5";
var tree =Hdf5.ReadTreeFileStructure(fileName);
var flat = Hdf5.ReadFlatFileStructure(fileName);
```

## Additional settings
 - Hdf5EntryNameAttribute: control the name of the field/property in the h5 file:
 
 ```csharp
     [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    public sealed class Hdf5EntryNameAttribute : Attribute
    {
        public string Name { get; }
        public Hdf5EntryNameAttribute(string name)
        {
            Name = name;
        }
    }
```

example:  
```csharp
private class TestClass : IEquatable<TestClass>
        {
            public int TestInteger { get; set; }
            public double TestDouble { get; set; }
            public bool TestBoolean { get; set; }
            public string TestString { get; set; }
            [Hdf5EntryNameAttribute("Test_time")]
            public DateTime TestTime { get; set; }
        }
```

 - Time and fields names in H5 file:
 
 ```csharp
     public class Settings
    {
        public DateTimeType DateTimeType { get; set; }
        public bool LowerCaseNaming { get; set; }
    }

    public enum DateTimeType
    {
        Ticks,
        UnixTimeSeconds,
        UnixTimeMilliseconds
    }
    
```

usage:
 ```csharp
        [ClassInitialize()]
        public static void ClassInitialize(TestContext context)
        {
            Hdf5.Hdf5Settings.LowerCaseNaming = true;
            Hdf5.Hdf5Settings.DateTimeType = DateTimeType.UnixTimeMilliseconds;
        }
            
```

 - Logging: use can set logging callback via: Hdf5Utils.LogError, Hdf5Utils.LogInfo, etc
 
 ```csharp
public static class Hdf5Utils
    {
        public static Action<string> LogError;
        public static Action<string> LogInfo;
        public static Action<string> LogDebug;
        public static Action<string> LogCritical;
    }
 ```


in order to log errors use this code snippet:
```csharp
            Hdf5.Hdf5Settings.EnableErrorReporting(true);
            Hdf5Utils.LogWarning = (string s) => {...}
            Hdf5Utils.LogCritical = (string s) => {...}
            Hdf5Utils.LogError = (string s) => {...}
```

## History

### V1.17.0 (25.03.2023):
- Update Breaking Change inside Dependecy PureHDF (previously HDF5.NET) 

### V1.16.3 (25.01.2023):
- Update Dependecy PureHDF (previously HDF5.NET)

### V1.16.2 (18.11.2022):
- File close problem #249
- Hdf5.WriteCompounds is missing the write attributes fix (move write to created compound name and not group id) #248

### V1.16.1 (12.11.2022):
- Hdf5.WriteCompounds is missing the write attributes fix #248

### V1.16.0 (11.09.2022):
- Add net 7 target framework.

### V1.15.4.1 (03.08.2022):
- Replace net.6.0-windows target with net6.0.

### V1.15.3 (23.07.2022):
- https://github.com/LiorBanai/HDF5-CSharp/issues/224: Memory increased #224

### V1.15.2 (14.07.2022):
- https://github.com/LiorBanai/HDF5-CSharp/issues/213: [NET5+] Add support for newly added primitive types #213
- https://github.com/LiorBanai/HDF5-CSharp/issues/216: Nullable support for "ReadObject" #216 (additional changes)

### V1.15.1:
 - https://github.com/LiorBanai/HDF5-CSharp/issues/219: Add Support for Reference Nullable Types #219

### V1.15.0:
 - https://github.com/LiorBanai/HDF5-CSharp/issues/216: Nullable support for "ReadObject" #216

### V1.14.3:
 - https://github.com/LiorBanai/HDF5-CSharp/issues/215: [BUG] Compound struct - missing memory release #215 

### V1.14.2: 
 - https://github.com/LiorBanai/HDF5-CSharp/issues/214: [BUG] OpenAttributeIfExists Tries to open dataset instead of attribute #214
 - https://github.com/LiorBanai/HDF5-CSharp/issues/212: [IMPROVEMENT] Add Throw if not exists boolean flag or Mandatory Attribute? #212

### V1.14.1: 
 - https://github.com/LiorBanai/HDF5-CSharp/issues/211: [IMPROVEMENT] don't try to open group if it does not exist #211