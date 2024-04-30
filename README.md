# Traverse And Encrypt Files wit XOR
How it works:
* Select a directory to start the TraverseFrom by clicking `ChooseRoot`
* The selected path should appear below.
* Write the desired extensions that you want to encryot (one by one) and add it by clicking `Add`
* It will appear int the list below and you can delete it with `Delete`
* By pressing `Encyrt` the Traverse will start and It will encrypt the files with the extensions found in the list below.
* You can modify the maximum number of threads by changing the Value of `MaxThreads` in `App.config`
* To Traverse through protected roots start in elevated mode(admin) 
```<requestedExecutionLevel  level="asInvoker" uiAccess="false" /> ``` .
* If you don't have the required privileges aapp shows MessageBox and closes.
> [!NOTE]  
> You can not set thread number above the physical cores of your computer.
