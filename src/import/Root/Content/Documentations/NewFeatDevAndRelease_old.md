# DEVELOP AND RELEASE A NEW FEATURE
This document focuses on releasing existing projects that are dependent on each other.
Creating projects based on SenseNet can be found in another document.

## Starter kit
- **repositoryA** currently in the develop branch.
  - **project1**: contains interfaces without implementations, `project1.nupkg` package need to be published.
  - **project3**: contains integration tests using implementations from repositoryB/project2, no package to publish.
- **repositoryB** currently in the develop branch.
  - **project2**: contains implementations of the repositoryA/project1, `project2.nupkg` package need to be published.
- **internal feed**: corporate nuget package storage
```
repositoryA        develop
  project1         1.0.0    <-- interfaces
  project3                  <-- test project
    ref: project2  1.0.0    <-- implementations
repositoryB        develop
  project2         1.0.0    <-- implementations
    ref: project1  1.0.0    <-- interfaces
internal feed:
  project1 1.0.0
  project2 1.0.0
```
## Develop a new feature
### Modify interfaces project
1. Create feature branch in `repositoryA`, name `feat1`.
2. Write code and test it well,
3. Raise internal version of the `project1` to `1.0.0.1` (see in details: [1])
4. Commit this with a message containing "`Raise internal versions`".

***Warning***: Tests (`project3`) cannot be compiled if the new api contains breaking changes.
For a simpler release process, this should not happen.
```
repositoryA        feat1    <-- 1.
  project1         1.0.0.1  <-- 3.
  project3
    ref: project2  1.0.0
repositoryB        develop
  project2         1.0.0
    ref: project1  1.0.0
internal feed:
  project1 1.0.0
  project2 1.0.0
```

### Internal release of interfaces
This step shares the "project1" package in the internal feed so that it can be used in any other project.
1. Create a pull request with a message summarizing the feature and containing "Upgrading internal versions".
2. Squash merge to develop. This operation trigger the internal release (see in details: [2]).
3. Delete feature branch
4. Check pipelines and internal feed (see in details: [3] and [4])
```
repositoryA        develop
  project1         1.0.0.1
  project3
    ref: project2  1.0.0
repositoryB        develop    <-- 2.
  project2         1.0.0
    ref: project1  1.0.0
internal feed:
  project1 1.0.0, 1.0.0.1     <-- 4.
  project2 1.0.0
```

### Modify implementation
In this step, we will use the newly released internal package.

1. Create feature branch `feat1`.
2. Update 'project1' reference from `1.0.0` to `1.0.0.1`
3. Write code, test, test, test.
4. Raise `project2`'s internal version to `1.0.0.1` (see in details: [1])
5. Commit with a message containing "`Raise internal versions`".
```
repositoryA        develop
  project1         1.0.0.1
  project3
    ref: project2  1.0.0
repositoryB        feat1      <-- 1.
  project2         1.0.0.1    <-- 4.
    ref: project1  1.0.0.1    <-- 2.
internal feed:
  project1 1.0.0, 1.0.0.1
  project2 1.0.0
```

### Internal release of implementation
1. Create PR: "Summary the feat + 'Raise internal versions'".
2. Squash merge to develop.
3. Delete feature branch
4. Check pipelines and internal feed (see in details: [3] and [4])
```
repositoryA        develop
  project1         1.0.0.1
  project3
    ref: project2  1.0.0
repositoryB        develop    <-- 2.
  project2         1.0.0.1
    ref: project1  1.0.0.1
internal feed:
  project1 1.0.0, 1.0.0.1
  project2 1.0.0, 1.0.0.1     <-- 4.
```

### Modify test project
1. Update `project2` reference in the `project3` to `1.0.0.1`,
2. Run tests,
3. Commit

**Note:**  If the tests need to be rewritten, create new feature branch (PR, squash merge etc.)
```
repositoryA        develop
  project1         1.0.0.1
  project3
    ref: project2  1.0.0.1    <-- 1.
repositoryB        develop
  project2         1.0.0.1
    ref: project1  1.0.0.1
internal feed:
  project1 1.0.0, 1.0.0.1
  project2 1.0.0, 1.0.0.1
```

## Release public packages of the new feature

### Prepare interfaces
1. In the develop branch raise public version: `1.0.0.1` --> `1.0.1`
2. Commit with message: "Raise public version". This starts the appropriate CI/CD pipeline
3. Check pipelines and internal feed (see in details: [3] and [4])
```
repositoryA        develop
  project1         1.0.1      <-- 1.
  project3
    ref: project2  1.0.0.1
repositoryB        develop
  project2         1.0.0.1
    ref: project1  1.0.0.1
internal feed:
  project1 1.0.0, 1.0.0.1, 1.0.1   <-- 3.
  project2 1.0.0, 1.0.0.1
```

### Prepare implementations
1. In the develop branch update reference to `1.0.1`
2. Raise public version of the `project2`: `1.0.0.1` --> `1.0.1`
2. Commit with message: "Raise public version". This starts the appropriate CI/CD pipeline
3. Check pipelines and internal feed (see in details: [3] and [4])
```
repositoryA        develop
  project1         1.0.1
  project3
    ref: project2  1.0.0.1
repositoryB        develop
  project2         1.0.1      <-- 2.
    ref: project1  1.0.1      <-- 1.
internal feed:
  project1 1.0.0, 1.0.0.1, 1.0.1
  project2 1.0.0, 1.0.0.1, 1.0.1   <-- 4.
```

### Update `test` project
1. Update reference
2. commit
```
repositoryA        develop
  project1         1.0.1
  project3
    ref: project2  1.0.1      <-- 1.
repositoryB        develop
  project2         1.0.1
    ref: project1  1.0.1
internal feed:
  project1 1.0.0, 1.0.0.1, 1.0.1
  project2 1.0.0, 1.0.0.1, 1.0.1
```

### Release interfaces package
These steps can be performed using shell commands. They no longer require VS because the develop branch is prepared for public release.

1. Merge to master (not squash, no delete the `develop` branch)
1. Add a brand new tag (see in details: [5])
1. Push (this step starts the github CI/CD action)
1. Check github action and public feed (see in details: [6] and [7])

The full shell command list of this step (just an example):
``` powershell
git checkout develop
git pull
git checkout master
git pull
git merge develop
git tag v4.1.0
git push
```

### Release implementations package
1. Merge to master (not squash, no delete the `develop` branch)
1. Add a brand new tag (see in details: [5])
1. Push (this step starts the github CI/CD action)
1. Check github action and public feed (see in details: [6] and [7])

## Release source code of the interfaces repository
1. Go to `Releases` page the `repositoryA`:
   https://github.com/...yourorganization.../repositoryA/releases]
1. Click `Draft a new release`
1. `Choose a tag`: create a brand new tag
1. Set `Target:` master / main
1. Set `Previous tag:` auto
1. Click `Generate release notes`
1. Edit `Release title` if needed
1. Edit release notes (see in details: [8])
1. Uncheck `Set as a pre-release`
1. Check `Set as the latest release`
1. Click `Publish release`

## Release source code of the implementations repository
1. Go to `Releases` page the `repositoryA`:
   https://github.com/...yourorganization.../repositoryB/releases]
1. Continue as above with the description of "Release source code of the interfaces repository"

## Notify staff about the new release
On Teams and/or email etc.

-------------------------------------------------
-------------------------------------------------
-------------------------------------------------

# Detailed description of elements

## [1] Raise internal version of a project
In sensenet we use semantic versioning in the public releases, but pre-release is not used (https://semver.org/). Public version is MAJOR.MINOR.PATCH e.g. `7.11.2`.
The internal version in sensenet means a four segment version number where the last number is greater than 0 (this is the build or revision number). The main rule is that only 3-digit versions can be released publicly, 4-digit versions are only visible in internal company feeds.

Increasing the internal version means increasing the last member of the four-member version number by one.
For example: original version: `7.6.1`, raised internal version: `7.6.1.1`.
This step is necessary so that the modification can be tested before the public release.

The package version is in the project file, and when upgrading, the build automatically generates a new package corresponding to the new version.
During development, multiple internal versions may be released if the first changes are not suitable.
Here is a part of a project file that has a multiple incremented internal version:
``` xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>...
    <Version>1.0.1.3</Version>
```
It is important that the commit message after this modification contains information about the raise of the internal version e.g.:
`Raise internal versions.`

## [2] Internal release of a project
Merging the feature branch to the `develop` branch activates a CI/CD pipeline
 that compiles the project and creates a package with the specified version, 
 and places it into the company's internal package store.

## [3] Check pipelines
After starting the internal release, the progress can be tracked
[on the recent pipelines page](https://tfs.sensenet.com/SenseNetProductTeam/SenseNet/_build "Recent pipelines of SenseNet").
The pipeline does not start immediately after the merge commit, there may be a delay of a few minutes if the build server is under load.

## [4] Check internal feed
The last step of an internal release is the package publishing. The new package existence can be
checked 
[on the SenseNet artifacts page](https://tfs.sensenet.com/SenseNetProductTeam/SenseNet/_artifacts/feed/SenseNet "SenseNet artifacts").
Use the `Filter by keywords` textbox to make it easier to find the specific package.
If the release is finished successfully, the package's version will be the last committed value.
There is another feed on
[this page](https://developerfeed.sensenet.cloud/ "SenseNet developer feed").
to check the new version.

## [5] Add a brand new tag
The new tag belongs to the repository and not to the released packages.
So there are no strict rules here, just one: each tag should be unique.
Here are some possible options for generating.
- The tag could refer to the most important change in the release ("healt-checker")
- The tag can contain the release date ("preview-2025-01-05")
- The tag could be the most important package version that has just been released ("v7.8.11"). In this case, it is recommended to use the prefix "v".

## [6] Check github action
Go to action page of the the github repository, for example 
[SenseNet.Client actions](https://github.com/SenseNet/sn-client-dotnet/actions "sn-client-dotnet actions"). See the first item of the list that's icon is yellow if the pipeline is in "preparation" state, rotating blue if running, and green if finished well, or red if there was any errors. If the build is faulty, clicking on the element will take you to the full log, where the causes can be identified.

If github-pipeline doesn't run and complains about NuGet apikey:
*"Response status code does not indicate success: 403 (The specified API key is invalid, has expired, or does not have permission to access the specified package.)"*, then rule out all possible problems (bad package name, version number), and finally check if the apikey is really expired and update it if necessary (see: [9])

## [7] Check public feed
Check the latest package version of the `SenseNet.Client` (*this package only an example!*)
[on the nuget.org](https://www.nuget.org/packages/SenseNet.Client "nuget.org").
The appearance of the new version number must be monitored by intensive use of F5 (manual polling 😁).
Sometimes it takes a few minutes.
Sometimes warnings appear, they disappear over time:
"This package has not been indexed yet..." or
"The symbols for this package have not been indexed yet..."

## [8] Edit release notes
GitHub can generate release notes from the pull request titles that have been created since the last release (last tag).
This is very useful, but it always needs to be edited because each line needs to be self-explanatory. This is the answer to the question of why an informative pull request title is needed. Another reason  for editing that the lines need to be grouped appropriately. The recommended main groups: *Features*, *Bug fixes*, *Other Changes*, but other things are also possible if there are many changes: *Documentation*, *Tests and Infrastructure*, etc.

## [9] Check and set Nuget ApiKey on github.com
Github needs an API key to publish the newly generated packages to the nuget.org.
The nuget's API key is generated on nuget.org but stored on github.
To perform the following workflows, you need the highest privileges on both 
nuget.org (Administrator) and github.com (owner).

### Check the Nuget apikey
1. Go to https://nuget.org
1. Login with your Microsoft account
1. Click next to your name (top right)
1. Select `API Keys`/`Manage`
1. See the expiration on the `snteam` card.
1. If no problem (e.g. "Expires in a year"), continue this workflow, else break and do the 'Generate Nuget apikey' workflow.
1. Go to https://github.com/SenseNet
1. Settings / Security / Secrets and variables / Actions
1. Check the `Last update` column. 
1. Based on these, decide whether to modify the apikey

### Generate Nuget apikey
If the key is outdated, this workflow describes how can you generate and store a new one.

1. Go to https://nuget.org
1. Login with your Microsoft account
1. Click next to your name (top right)
1. Select `API Keys`/`Manage`
1. Click `Regenerate` in the `snteam` card.
1. Click `Copy` in the `snteam` card.
1. Go to https://github.com/SenseNet
1. Settings / Security / Secrets and variables / Actions
1. Click Edit icon on the `NUGET_API_KEY` line.
1. Click "`enter a new value`"
1. Paste the nuget apikey value and click `Save changes`.
1. Authenticate yourself if needed.

## Troubleshooting
### Secret key collision
If you get the following error even though the Nuget apikey has been updated
*"Response status code does not indicate success: 403 (The specified API key is invalid, has expired, or does not have permission to access the specified package.)"*
Ensure that the organization secret key is not overridden in repository level.
For example check for `sn-tools`: https://github.com/SenseNet/sn-tools/settings/secrets/actions
Ensure that the `NUGET_API_KEY` of the `Organization secrets` is not overridden in the `Environment secrets` and the `Repository secrets` sections.
### Too generic package name
The `dotnet nuget push` command may fail if the package name is too generic e.g. `Tests`. See this log part:
```
Pushing Tester.1.2.0.nupkg to 'https://www.nuget.org/api/v2/package'...
  PUT https://www.nuget.org/api/v2/package/
  Forbidden https://www.nuget.org/api/v2/package/ 114ms
error: Response status code does not indicate success: 403 (The specified API key is invalid, has expired, or does not have permission to access the specified package.).
```
The error means that you cannot push the existing `Tester` package because this package is probably not yours, so the APIKEY you set does not allow this operation. Solution:
- Choose unique package name
- Use solution filter file (*.slnf) in the github actions.