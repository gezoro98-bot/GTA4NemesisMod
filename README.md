name: Build mod + installer

on:
  push:
  workflow_dispatch:

permissions:
  contents: read

jobs:
  build:
    runs-on: windows-latest
    steps:
      - name: Checkout
        uses: actions/checkout@v4

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '6.0.x'

      - name: Install NSIS
        run: choco install nsis -y --no-progress

      - name: Build (auto-downloads ScriptHookDotNet + builds + NSIS)
        shell: pwsh
        run: ./build.ps1

      - name: Upload artifacts
        uses: actions/upload-artifact@v4
        with:
          name: GTA4-Nemesis-Release
          path: dist/
          if-no-files-found: error
