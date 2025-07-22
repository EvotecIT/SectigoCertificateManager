#!/usr/bin/env bash
# Demonstrates downloading a certificate as PFX using the CLI.

dotnet run --project ../../SectigoCertificateManager.CLI download-pfx 123 ./cert.pfx secret
