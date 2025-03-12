# Vinnare Documentation

## Product Overview
Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nulla vehicula, urna sed tincidunt vestibulum, velit quam feugiat nisi, et placerat lectus neque at sapien.

## How to Run the Project
To run the project, ensure you are in the **root folder**.

### Using Docker:
```sh
docker build . -f ./Api/Dockerfile
```

### Running Locally:
```sh
dotnet restore
dotnet build
dotnet Api/bin/Debug/net8.0/Api.dll
```

## Documentation Structure
This documentation is organized into multiple sections:

- [How to Run the Project](how_to_run.md)
- [CI/CD Pipelines](./code/ci_cd.md)
- [Diagrams](./diagrams/diagrams.md)
- [Technical Issues](technical_issues.md)
- [Code](./code/code.md)
- [Features](features.md)

Refer to the respective pages for detailed information.