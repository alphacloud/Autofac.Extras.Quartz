Contributing
============

## Pull requests

- Add XML documentation to all public members.
- Add unit-tests covering functionality implemented (unless this is trivial fix or logic was not changed).
- Make sure all tests are passing.
- Squash commits. Ideally it should be 1 commit per feature implemented or big fixed). If there are several features/bugs 
addressed by pull request, consider creating several PRs.
- Create pull request against **develop** branch.


## Dependencies upgrade
When NUnit.ConsoleRunner is upgraded make sure **build.proj/Test** target has correct path to the **nunit.console**.
