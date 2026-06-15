---
status: accepted
---

# Snapshot vs living entities

Weekly Pulse data splits into two classes. **Snapshot entities** (KPI results, check-ins, headlines, weekly summary, meeting rating) belong to exactly one Week and are blank at the start of each Week. **Living entities** (Issues, To-Dos) are created *in* a Week but persist and resurface in every later Weekly Pulse Review until they reach a terminal state (Issue → Solved, To-Do → Done). A living entity is never copied forward — it keeps a single identity across all the Weeks it touches.

## Why

The spec says "each new week starts blank," which reads as a strict weekly partition over all data. But it also requires the Review to surface **Open** issues and **Overdue** to-dos — impossible under a strict per-Week partition without a fragile copy-forward job that duplicates records every Monday. Splitting the model resolves the contradiction: "blank each Week" applies to *snapshots*; the *living* lists persist. This also matches the EOS/Ninety mental model the product is based on — to-dos and issues are a rolling list, while check-ins and KPI numbers are weekly snapshots.

The Meeting History "minutes" view (a living entity appearing under every Week it was active in) depends on each Issue/To-Do having **one identity across Weeks**, which only the living model provides.

## Considered options

- **Model A — everything Week-scoped, unresolved items copied forward each Week.** Rejected: duplicates records, needs a scheduled rollover job, and shatters the cross-Week identity of an issue/to-do, which the minutes view and back-links (Conversion) rely on.
