# About
This example exercises DataGridView in a way that is similar to Windows Explorer's list view and other popular file listing apps like voidtools Everything.

![main](https://github.com/MikeClark512/WinFormsDataGridViewFileExplorer/blob/master/screenshot.png)

## Goals
- Brevity: The non-designer code is intentionally brief, at around 200 lines.
- Plain objects: The code should be easily portable to work with any plain old objects (e.g. deserialized JSON). There is no direct depenency on FileSystemInfo.
- Fluent gestures and responsive behavior

## Column and Window Resizing
- The columns should respond to the form window being resized. (This behavior is questionable. Windows Explorer doesn't do this, and the behavior has pros and cons.)
- Fill all space made available to the control when the parent window resizes. (Also questionable. See above.)
- The starting column widths should be auto-sized in a way that respects the lengths of all initial strings in the column. (There is some trouble with this -- what if you add more data that's longer, or scroll down to an item that's longer? Also, there are performance concerns with very long lists.)
- The user should be able to resize the columns.
- If the window is resized after the columns are resized, the columns should resize proportionally to each other. (This behavior is questionable. Windows Explorer doesn't do this, and the behavior has pros and cons.)

## Plain Old Data
- We should be able to populate the DataGridView programatically from a plain data-type object, like a deserialized JSON object.
- Population of the DataGridView's metadata (like columns) and data (like cell contents) should not need any funky database/ADO wiring.
- We should be able to update the DataGridView while the application is running and not get bad behaviors like the DGV scrolling to the top or bottom (so the user loses their place), or redraw issues.
- The plain old object backing the DGV should be retrievable from the DGV

## Sorting
- User-controlled column sorting
- Click a column header to switch between sorting by that column: ascending/descending

## Well-known keyboard gestures should work as expected
- Press <kbd>enter</kbd> to open the currently selected row (directory/file)
- Press <kbd>enter</kbd> to confirm input ("OK") in single-line text boxes and dialogs, etc.
- Up and down arrows should move focus between nearby controls in away similar to <kbd>Tab</kbd>
- Explorer-isms: Alt-Up to "cd .."

## Well-known mouse gestures should work as expected
- Double-click to open files and directories
- Click to select things
- Scroll-wheel to scroll
- Scrollbar positions do not reset unexpectedly

## Data Selection and Scrolling
- Data selection in this project is assumed to be full-row
- Only one row is selectable at a time. This met my current needs.
- The backing data (plus metadata) should be easily retrievable given the current state of user selection

## Future
- Fluent support for touch-screen gestures
- Icon support (maybe)
- Multi-select (maybe)
- Context menu (maybe)
- Inline renaming
