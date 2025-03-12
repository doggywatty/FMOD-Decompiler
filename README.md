# FMOD-Decompiler

FMOD-Decompiler is a WIP tool that attempts to decompile multiple FMOD bank files into a single `.fspro` project. This allows users to reconstruct an FMOD project from compiled bank files, making it easier to analyze or modify existing audio assets.<br>


This uses DLLs from the [FMOD Studio API](https://fmod.com/download#fmodengine).

## Features
- Parses and extracts data from FMOD bank files (`.bank`)
- Attempts to reconstruct an FMOD Studio Project (`.fspro`)
- Supports multiple bank files for a unified project

## Installation
1. Clone this repository:
   ```sh
   git clone https://github.com/stuttermess/FMOD-Decompiler.git
   cd FMOD-Decompiler
   ```
2. Build the project using .NET (if applicable):
   ```sh
   dotnet build
   ```

## Usage
1. Run the executable with the necessary arguments:
```sh
FMOD-Decompiler --input "path/to/bank/folder" --output "path/to/output/project"
```
2. Wait for the Program to finish, then open the Project in FMOD Studio

3. Then once the project loads, go to File -> Validate Project...

4. Validate the entire project and Save the project

### Arguments:
- `--input`: Path to the folder containing FMOD bank files.
- `--output`: Destination folder for the generated `.fspro` project.
- `--verbose`: (Optional) Enables detailed logging.
- `--safeorglevel1`: (Optional) Enables Experimental Automatic Project Organization. (Only use when Organization issues arise)
- `--safeorglevel2`: (Optional) Disables Automatic Project Organization.

## Limitations
- The tool is an attempt to reconstruct `.fspro` projects and may not be fully accurate.
- Some metadata or complex FMOD features may not be fully recovered.
- Compatibility with newer versions of FMOD Studio is not guaranteed.
- Bank files that have event subfolders with the same name under different folders are not fully supported (Consider using `--safeorglevel1` or `--safeorglevel2`).

## Contributing
Pull requests and issue reports are welcome! Feel free to contribute improvements or report bugs.

## License
This project is licensed under the GNU License. See `LICENSE` for more details.

