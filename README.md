# POS Register

WPF desktop register for the single-store POS — the selling head of the system. Talks to the
`inv-management-w-pos-backend` API over HTTP on the same machine; the web admin
(`inv-management-w-pos-frontend`) is the management head.

## Commands

    dotnet build                          # build
    dotnet run --project src/POS.Register # run the register (1920x1080 canvas, scales to the monitor)
    node scripts/check-architecture.mjs   # dependency lock + slice grammar
    node scripts/check-design.mjs         # design-token drift + raw-color scan
    node scripts/check-conventions.mjs    # commit/branch conventions (PR-only in CI)

Current state: static navigable shell — canned data, no API wiring yet.
