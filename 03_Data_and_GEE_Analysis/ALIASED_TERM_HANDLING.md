# Aliased-term handling in the confirmatory GEE

## Editor request

The Editor requested that the raw per-observation dataset and the GEE script be public, **including how the aliased term was handled**.

## Confirmatory model (manuscript / SAP Appendix E.5)

```
Outcome ~ Condition + Period + TaskPair + Condition × Period + Condition × Order
```

- Family: Gaussian  
- Working correlation: exchangeable  
- Clustering unit: `participant_id`  
- Reference coding: **Traditional** is the reference level for Condition, so the reported coefficient is the AI − Traditional contrast.

## Why aliasing occurs

In this counterbalanced crossover, Sequence, Period, Condition, and Order are linearly dependent under saturated specifications. Order is a deterministic function of Sequence (A/C = Traditional-first; B/D = AI-first). Entering Condition×Order together with Condition + Period therefore produces a rank-deficient design matrix — the same structural issue that led to singular mixed-effects fits in the manuscript.

## Handling rule (implemented in `gee_confirmatory_analysis.py`)

1. Construct the confirmatory design matrix from the SAP formula above, with Traditional as the Condition reference.
2. Do **not** enter Sequence as a separate main effect (Order already encodes the A/C vs B/D start-condition contrast).
3. Before GEE estimation, compute the numeric rank of the design matrix.
4. If columns are linearly dependent, drop aliased columns using a pivoted QR (SciPy) / SVD fallback and **log the dropped term names** to `gee_outputs/aliased_term_handling.json` and the per-outcome result records.
5. Estimate Gaussian GEE with exchangeable working correlation and `participant_id` clustering on the reduced full-rank design.
6. Report **only the main-effect Condition[AI]** coefficient as the adjusted AI effect (never an interaction term).
7. Treat GEE as confirmatory; the paired participant×task-pair contrast remains primary.

## What is dropped in practice

For the released dataset, the Condition×Order columns are the aliased terms removed by the QR pivot. After that reduction, the estimable confirmatory model that reproduces manuscript Table 6 is:

```
Outcome ~ Condition + Period + TaskPair + Condition × Period
```

The Condition×Order terms remain in the *specified* SAP formula; they are simply not estimable and are logged as aliased rather than forced into a singular fit.

## Reproducing Table 6

From this folder:

```bash
python gee_confirmatory_analysis.py
```

Expected main-effect AI coefficients (3 d.p.):

| Outcome | Adjusted AI effect |
|---------|--------------------|
| Completion time (min) | −5.283 |
| Test failures | −1.033 |
| Code smells | −1.371 |
| Cyclomatic complexity | −0.842 |
| Maintainability Index | +5.788 |
| Lines of code | +14.096 |
| NASA-TLX | −10.167 |
\n\n## Period-specific interpretation\n\nBecause Period 1 is the reference period, the Condition main-effect coefficient is the Period 1 AI-minus-Traditional contrast. The Period 2 contrast is calculated as the sum of the Condition main effect and the Condition×Period 2 interaction. The script exports both period-specific contrasts, their confidence intervals and p-values, as well as the Condition×Period interaction estimates. Holm adjustment is applied to the seven Period 1 GEE p-values reported in the manuscript table.\n