# Simple Enterprise Scheduler

Simple Enterprise Scheduler is an open-source application designed to execute and schedule automated jobs in a clear, maintainable, and developer-friendly way.

This project was born from the need to move away from bloated, overengineered schedulers and toward a solution that is straightforward, predictable, and easy to reason about. Whether you're running scripts on a server, orchestrating data processing tasks, or triggering infrastructure workflows, this scheduler aims to provide a focused experience built around one key principle: **run jobs well — no more, no less.**

This is not a library or a developer framework. It is a real application that can be hosted and extended to meet operational needs.

> ⚠️ **DISCLAIMER**
> This is not a production-ready system — yet. I'm building it piece by piece when time permits, because I find the challenge interesting and the space worth improving. If you're exploring alternatives to overcomplicated schedulers, feel free to watch, fork, or contribute as it grows into something usable.

Key traits:

- **Simplicity first**: Everything in this system is transparent and purpose-driven. If it’s not needed, it’s not there.
- **Structured and observable**: Logging is first-class. Errors are traceable. Behavior is explainable.
- **Architecture-aware**: While the system is powered by .NET and implements [Clean Cut Architecture](https://github.com/fernando-jimenez-dev/clean-cut-architecture), you don’t need to know any of that to use it effectively.

Whether you’re a developer deploying jobs, an ops engineer looking for reliable execution, or a curious technologist evaluating alternatives to heavy tools — this scheduler was made to get out of your way and just work.
