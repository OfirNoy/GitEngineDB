# GitEngineDB
A very simple storage library based on Git

Initialize an instance that will store data in 'dir':

```
var db = new GitDbEngine(dir, userName, userEmail);
```

API:

dataId - the identifier of the stored data 

Returns the .NET instance of the stored data

(JsonSerializer is used to convert the stored json to a .NET type T)

```
T GetData<T>(string dataId)
```

dataId - the identifier of the stored data 

Returns the stored string
```
string GetRawData(string dataId)
```

dataId - the identifier of the stored data 

data - the .NET instance to be stored 

If dataId is new, it will be added to the repository

If dataId exists, the new data will overwrite the currently stored content
```
void SetData<T>(string dataId, T data)
```

dataId - the identifier of the stored data 

data - the string data to be stored 

If dataId is new, it will be added to the repository

If dataId exists, the new data will overwrite the currently stored content
```
void UpdateData(string dataId, string data)
```
