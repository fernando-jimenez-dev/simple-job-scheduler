## Title

Implement {UseCaseName} Use Case

---

## Description

This PR implements the `{UseCaseName}` use case following Clean Cut Architecture principles.

### What’s Included

- Application-level orchestration in `{UseCaseClass}`.
- Defined input/output via records: `{InputRecord}`, `{OutputRecord}`.
- Domain error modeling via `ApplicationError` subtypes.
- Infrastructure abstractions (`IWhateverExecutor`, `ISomethingVerifier`) and basic concrete implementations.
- Minimal API endpoint exposing the use case through a consistent contract.
- HTTP response mapped via `{ResponseDto}` or `JsonHttpResult<T>`.

### Tests

- [ ] Happy path
- [ ] All expected failure modes
- [ ] Response DTO mapping
- [ ] API contract validation

### Manual QA

- [ ] Verified end-to-end execution
- [ ] Confirmed proper error propagation
- [ ] Observed correct HTTP codes and response shape

---

## Notes

Leave a short note on what's _not_ included, if anything is deliberately deferred.  
Example: "Retries, logging, and cancellation tokens are not yet implemented."
