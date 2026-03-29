# Lifecycle

In this article, we give the times at which DataManager operations run.

## Global data

* DataManager loads Global and Profile data during the Plugin.Start method.
It is likely that this will run before your plugin's Start method, but this is
not guaranteed by Unity.
If you need data this early, you may want to wait a frame before accessing the data,
although this should not be necessary for most pluggins.
* DataManager writes Global and Profile data to disk when the user quits the game.
Note - this will not happen if the game crashed.

## Save data

* When the user creates a new save file, DataManager will not do anything to SaveData mods.
It is your responsibility to construct an instance of your save data class, although this will be done
automatically if you use the example code (see <xref:ExamplesArticle>).
* When the user enters an existing save file, DataManager will set the SaveData
property to the save data from the previous time they entered the file. If there
was no save data then (e.g. if the user installed your mod after they created the save),
then DataManager will not do anything at this moment.
* Whenever the game saves, DataManager will get the SaveData property and write it to disk.
This will happen regardless of if your save data was loaded when the user entered the file.
* When the user quits to menu, DataManager will set the SaveData property to null
(after the save data gets saved to disk).
If you are using the code in the example, this will be intercepted
and a new SaveData object will be constructed (for if they enter a new file).
* When the user deletes a save file, all data managed by DataManager for that save file
will be deleted too.

## OnceSaveData

* When the user creates a new save file, the OnceSaveData will be retrieved and written to disk.
* Whenever the SaveData is loaded for a file (or set to null), the OnceSaveData will be as well.
