#!/usr/bin/env bash
# Builds and tests every sample solution (samples/*/*.slnx).
# Keeps going after a failure so one run shows every broken sample, then exits 1 if any failed.
#   ./scripts/test-samples.sh                  all samples
#   ./scripts/test-samples.sh kafka-partition-lab rabbitmq-routing-lab   only these
set -uo pipefail

cd "$(dirname "$0")/../samples"
mapfile -t names < <(if [ $# -gt 0 ]; then printf '%s\n' "$@"; else ls -d */ | sed 's#/##'; fi)

passed=(); failed=()
for name in "${names[@]}"; do
  solution=$(ls "$name"/*.slnx 2>/dev/null | head -1)
  if [ -z "$solution" ]; then echo "::warning::No .slnx found in samples/$name"; continue; fi
  echo "::group::$name"
  if (cd "$name" && dotnet test "$(basename "$solution")" --nologo -v minimal); then
    passed+=("$name")
  else
    failed+=("$name"); echo "::error title=Sample failed::samples/$name did not build or its tests failed"
  fi
  echo "::endgroup::"
done

summary="### Samples: ${#passed[@]} passed, ${#failed[@]} failed"
if [ ${#failed[@]} -gt 0 ]; then summary+=$'\n\nFailed:\n'; for f in "${failed[@]}"; do summary+="- \`$f\`"$'\n'; done; fi
echo "$summary"
[ -n "${GITHUB_STEP_SUMMARY:-}" ] && echo "$summary" >> "$GITHUB_STEP_SUMMARY"
[ ${#failed[@]} -eq 0 ]
