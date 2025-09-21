# ATM Banking System Demo

## Overview
This project simulates an ATM banking system with the following components:
- **ActiveX Control**: COM-visible control for HTA UI interaction
- **Mock XFS Manager**: Simulates hardware devices (card reader, PIN pad, cash dispenser, printer)
- **Host Simulator**: ISO-8583 TCP server for processing transactions
- **HTA UI**: HTML Application for ATM interface
- **GRG Controls**: Mocked GRG components

## Requirements
- Windows OS
- .NET Framework 4.8
- HTA host
- Visual Studio or MSBuild for building

## Quick Start
1. Run `scripts\build.bat` to build all projects
2. Run `scripts\install.bat` to register ActiveX components
3. Launch demo: `scripts\run-demo.bat`
4. Run `scripts\uninstall.bat` to clean up

## Directory Structure
- `src/` → Source code for all components
- `config/` → Device configuration, states, and test data
- `docs/` → Documentation
- `tests/` → Unit and integration tests
- `scripts/` → Build, install, run demo scripts
