# FolderDiff
> Recursively search for files that have been changed or added 

[![License](http://img.shields.io/:license-Unlicense-blue.svg?style=flat-square)](http://badges.mit-license.org)

## Synopsis
  FolderDiff [--exclude=\"EXCLUDE_PATH\"] PATH_A PATH_B            

## Description
The following differences between the two folders are returned: 

- Any files that are newer in PATH_A than in PATH_B
- Any files that are in PATH_A but not in PATH_B

Returns a list with all files (and their relative paths) that were found:            

- CHA xxxx <- (CHANGED) file in PATH_A is newer and different than in PATH_B
- MIS xxxx <- (MISSING) file is missing in PATH_B

## Options
### --ignore-times
Check for changes between files regardless of timestamp.

Even if files have the same timestamp or if the file in PATH_B is newer than
the file in PATH_A it will still be flagged as changed if its contents are
different.

### --exclude=\"EXCLUDE_PATH\"
You can exclude parts of your folder structure with the --exclude parameter.

Paths are matched against the start of a relative path (starting with a /).
This parameter can be specified multiple times.

## Examples
```
FolderDiff --exclude="/exclude_me" /test/a /test/b
```
Changes made in /test/a after /test/b are highlighted
Changes made in /test/b after /test/a are ignored
Any changes in /test/a/exclude_me are ignored.

## License

[![License](http://img.shields.io/:license-unlicense-blue.svg?style=flat-square)](http://badges.mit-license.org)

- **[The Unlicense](https://unlicense.org/)**