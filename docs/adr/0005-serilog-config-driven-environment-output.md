# Serilog in All Environments with Config-Driven Output

Chronoscope uses Serilog as the single logging pipeline in all environments, with behavior sourced from environment-specific configuration files. Development uses Serilog Console sink with standard readable console formatting, while non-development environments use Serilog JSON console output (non-compact) to support structured log collection; this supersedes ADR 0004.
