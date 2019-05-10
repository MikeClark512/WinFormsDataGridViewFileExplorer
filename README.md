# About
This example exercises DataGridView in a way that is similar to Windows Explorer's list view and other popular file listing apps like voidtools Everything.

## Goals
- Brevity: The non-designer code is intentionally brief, at around 200 lines.
- Plain objects: The code should be easily portable to work with any plain old objects (e.g. deserialized JSON), leaving behind any trace of FileSystemInfo.
- Fluent gestures and responsive behavior

## Column and Window Resizing
- The columns should respond to the form window being resized
- fill all space made available to the control when the parent window resizes
- The original column widths should be selected in a way that respects the lengths of all the strings in the column
- The user should be able to resize the columns.
- If the window is resized after the columns are resized, the columns should resize proportionally to each other.

## Plain Old Data
- We should be able to populate the DataGridView programatically.
- Population of the DataGridView should be done from a "plain old object" and not need any funky database/ADO wiring.
- We should be able to update the DataGridView while the application is running and not have it look strange.
- A plain old object class should be retrievable

## Sorting
- User-controlled column sorting
- Click a column header to switch between sorting by that column: ascending/descending

## Well-known keyboard gestures should work as expected
- Keys.enter to open directory/file and confirm input
- Up and down arrows should move focus between nearby controls in away similar to TABing
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