import { useEffect, useMemo, useState } from "react";
import { useQuery } from "@tanstack/react-query";
import {
  AreaChart,
  Area,
  LineChart,
  Line,
  ComposedChart,
  Bar,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  ResponsiveContainer,
  ReferenceLine,
  Legend,
} from "recharts";
import { getScenarios, getReport, type ScenarioInfo } from "@/api/stress";
import {
  useStressSimulation,
  type TickPayload,
  type ScenarioEndPayload,
  type ForecastPayload,
  type RecommendationPayload,
} from "@/hooks/useStressSimulation";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { cn } from "@/lib/utils";

export default function StressSimulationPage() {
  const { data: scenarios } = useQuery({
    queryKey: ["stress-scenarios"],
    queryFn: getScenarios,
  });

  const [selected, setSelected] = useState<Set<string>>(new Set());
  const [reportMarkdown, setReportMarkdown] = useState<string | null>(null);

  const [retailersIdx, setRetailersIdx] = useState(4);
  const [inventoryIdx, setInventoryIdx] = useState(5);
  const [menusIdx, setMenusIdx] = useState(4);
  const [usersIdx, setUsersIdx] = useState(3);

  const {
    isRunning,
    currentScenario,
    ticks,
    scenarioResults,
    forecasts,
    recommendations,
    reportAvailable,
    runId,
    error,
    start,
    stop,
  } = useStressSimulation();

  useEffect(() => {
    if (reportAvailable && runId) {
      getReport(runId).then(setReportMarkdown).catch(() => {});
    }
  }, [reportAvailable, runId]);

  const toggleScenario = (name: string) => {
    setSelected((prev) => {
      const next = new Set(prev);
      if (next.has(name)) next.delete(name);
      else next.add(name);
      return next;
    });
  };

  const handleStart = () => {
    setReportMarkdown(null);
    start({
      scenarioNames: Array.from(selected),
      retailers: RETAILER_PRESETS[retailersIdx],
      inventoryItems: INVENTORY_PRESETS[inventoryIdx],
      menus: MENU_PRESETS[menusIdx],
      concurrentUsers: USER_PRESETS[usersIdx],
    });
  };

  const latestTick = ticks[ticks.length - 1] ?? null;

  const progress = currentScenario
    ? Math.min(
        ((latestTick?.elapsedSeconds ?? 0) / currentScenario.durationSeconds) *
          100,
        100
      )
    : 0;

  return (
    <div className="space-y-6">
      <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <h1 className="text-2xl font-bold tracking-tight">
            Stress Simulation Dashboard
          </h1>
          <p className="text-sm text-muted-foreground">
            Live infrastructure stress testing visualization
          </p>
        </div>
        <div className="flex gap-2">
          <Button
            onClick={handleStart}
            disabled={isRunning}
            className="min-w-[100px]"
          >
            {isRunning ? "Running…" : "Start"}
          </Button>
          <Button
            variant="outline"
            onClick={stop}
            disabled={!isRunning}
            className="min-w-[100px]"
          >
            Stop
          </Button>
        </div>
      </div>

      {error && (
        <div className="rounded-md border border-red-200 bg-red-50 p-3 text-sm text-red-700">
          {error}
        </div>
      )}

      <div className="rounded-lg border p-5 space-y-4">
        <h3 className="text-sm font-semibold text-muted-foreground uppercase tracking-wide">
          Load Profile
        </h3>
        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
          <PresetSlider
            label="Retailers"
            presets={RETAILER_PRESETS}
            index={retailersIdx}
            onChange={setRetailersIdx}
            disabled={isRunning}
          />
          <PresetSlider
            label="Inventory Items"
            presets={INVENTORY_PRESETS}
            index={inventoryIdx}
            onChange={setInventoryIdx}
            disabled={isRunning}
          />
          <PresetSlider
            label="Menus"
            presets={MENU_PRESETS}
            index={menusIdx}
            onChange={setMenusIdx}
            disabled={isRunning}
          />
          <PresetSlider
            label="Concurrent Users"
            presets={USER_PRESETS}
            index={usersIdx}
            onChange={setUsersIdx}
            disabled={isRunning}
          />
        </div>
      </div>

      <ScenarioSelector
        scenarios={scenarios ?? []}
        selected={selected}
        onToggle={toggleScenario}
        disabled={isRunning}
      />

      {currentScenario && (
        <ScenarioProgress
          scenario={currentScenario}
          progress={progress}
          elapsed={latestTick?.elapsedSeconds ?? 0}
        />
      )}

      {ticks.length > 0 && (
        <>
          <div className="grid grid-cols-1 gap-4 lg:grid-cols-3">
            <ChartCard title="API Latency (ms)">
              <ApiLatencyChart ticks={ticks} />
            </ChartCard>
            <ChartCard title="Kafka Consumer Lag">
              <KafkaLagChart ticks={ticks} />
            </ChartCard>
            <ChartCard title="Debezium CDC Queue">
              <CdcQueueChart ticks={ticks} />
            </ChartCard>
            <ChartCard title="Elasticsearch Indexing">
              <EsIndexingChart ticks={ticks} />
            </ChartCard>
            <ChartCard title="Redis Memory">
              <RedisMemoryChart ticks={ticks} />
            </ChartCard>
            <ChartCard title="Backpressure Level">
              <BackpressureChart ticks={ticks} />
            </ChartCard>
          </div>

          {latestTick && <ComponentStatusTable tick={latestTick} />}
        </>
      )}

      {scenarioResults.length > 0 && forecasts.length > 0 && (
        <ExecutiveSummary
          scenarioResults={scenarioResults}
          forecasts={forecasts}
          recommendations={recommendations}
        />
      )}

      {scenarioResults.length > 0 && (
        <ScenarioResultsTable results={scenarioResults} />
      )}

      {forecasts.length > 0 && <ForecastTable forecasts={forecasts} />}

      {recommendations.length > 0 && (
        <RecommendationsTable recommendations={recommendations} />
      )}

      {reportMarkdown && <ReportSection markdown={reportMarkdown} />}
    </div>
  );
}

// ────────────────── Stakeholder-friendly mappings ──────────────────

const FRIENDLY_NAMES: Record<string, string> = {
  PostgreSQL: "Database",
  "Debezium CDC": "Data Sync Pipeline",
  Kafka: "Message Queue",
  Elasticsearch: "Search Engine",
  Redis: "Cache",
  "API Load Generator": "Application Layer",
};

const FRIENDLY_ICONS: Record<string, string> = {
  Database: "cylinder",
  "Data Sync Pipeline": "arrow-right-left",
  "Message Queue": "mailbox",
  "Search Engine": "search",
  Cache: "zap",
  "Application Layer": "globe",
};

function friendlyName(tech: string): string {
  return FRIENDLY_NAMES[tech] ?? tech;
}

type HealthLevel = "healthy" | "caution" | "critical";

function healthFromSaturation(sat: number): HealthLevel {
  if (sat < 0.3) return "healthy";
  if (sat < 0.7) return "caution";
  return "critical";
}

function healthLabel(h: HealthLevel): string {
  return h === "healthy" ? "Healthy" : h === "caution" ? "Needs Attention" : "At Risk";
}

function healthColor(h: HealthLevel): string {
  return h === "healthy"
    ? "bg-green-100 text-green-800 border-green-200"
    : h === "caution"
      ? "bg-amber-50 text-amber-800 border-amber-200"
      : "bg-red-50 text-red-800 border-red-200";
}

function healthDot(h: HealthLevel): string {
  return h === "healthy"
    ? "bg-green-500"
    : h === "caution"
      ? "bg-amber-500"
      : "bg-red-500";
}

function scalingVerdict(f: ForecastPayload): {
  label: string;
  detail: string;
  health: HealthLevel;
} {
  if (f.estimatedStable && f.backpressureLevel < 0.3)
    return {
      label: "Ready",
      detail: "The platform can comfortably handle this level of traffic.",
      health: "healthy",
    };
  if (f.estimatedStable)
    return {
      label: "Possible with tuning",
      detail: `Stable but under pressure. ${friendlyName(f.topBottleneck)} will need attention.`,
      health: "caution",
    };
  return {
    label: "Not ready",
    detail: `System will become overloaded. The main constraint is ${friendlyName(f.topBottleneck)}.`,
    health: "critical",
  };
}

function scenarioVerdict(r: ScenarioEndPayload): {
  label: string;
  detail: string;
  health: HealthLevel;
} {
  const errorRate = r.totalWrites > 0 ? r.totalErrors / r.totalWrites : 0;
  if (r.peakBackpressure < 0.3 && errorRate < 0.01)
    return {
      label: "Passed",
      detail: "All systems performed well under this workload.",
      health: "healthy",
    };
  if (r.peakBackpressure < 0.7 && errorRate < 0.05)
    return {
      label: "Passed with warnings",
      detail: `Some pressure observed${r.bottlenecks[0] ? ` in ${friendlyName(r.bottlenecks[0].component)}` : ""}.`,
      health: "caution",
    };
  return {
    label: "Failed",
    detail: `System could not keep up. ${r.alertCount} alert${r.alertCount !== 1 ? "s" : ""} triggered.`,
    health: "critical",
  };
}

function simplifyRecommendation(r: RecommendationPayload): string {
  const comp = friendlyName(r.component);
  const action = r.reason.length > 80 ? r.reason.slice(0, 77) + "..." : r.reason;
  return `${comp}: ${action}`;
}

function deriveOverallHealth(
  scenarios: ScenarioEndPayload[],
  forecasts: ForecastPayload[]
): { health: HealthLevel; summary: string } {
  const worstScenarioBp = Math.max(...scenarios.map((s) => s.peakBackpressure));
  const totalErrors = scenarios.reduce((sum, s) => sum + s.totalErrors, 0);
  const totalWrites = scenarios.reduce((sum, s) => sum + s.totalWrites, 0);
  const errorRate = totalWrites > 0 ? totalErrors / totalWrites : 0;
  const twoX = forecasts.find((f) => f.multiplier === 2);
  const fiveX = forecasts.find((f) => f.multiplier === 5);

  if (worstScenarioBp < 0.3 && errorRate < 0.01 && twoX?.estimatedStable)
    return {
      health: "healthy",
      summary:
        "The platform is performing well under all tested scenarios and can handle at least double the current traffic.",
    };
  if (worstScenarioBp < 0.7 && errorRate < 0.05)
    return {
      health: "caution",
      summary:
        "The platform handles current load but shows signs of pressure under peak scenarios. Tuning is recommended before scaling further.",
    };
  return {
    health: "critical",
    summary:
      "The platform struggles under high-load scenarios. Immediate infrastructure improvements are needed before any traffic increase.",
  };
}

// ────────────────── Executive Summary Component ──────────────────

function ExecutiveSummary({
  scenarioResults,
  forecasts,
  recommendations,
}: {
  scenarioResults: ScenarioEndPayload[];
  forecasts: ForecastPayload[];
  recommendations: RecommendationPayload[];
}) {
  const overall = useMemo(
    () => deriveOverallHealth(scenarioResults, forecasts),
    [scenarioResults, forecasts]
  );

  const componentHealth = useMemo(() => {
    const map = new Map<string, { maxSat: number; errors: number }>();
    for (const sr of scenarioResults) {
      for (const b of sr.bottlenecks) {
        const existing = map.get(b.component);
        if (!existing || b.maxSaturation > existing.maxSat) {
          map.set(b.component, {
            maxSat: b.maxSaturation,
            errors: b.totalErrors,
          });
        }
      }
    }
    return Array.from(map.entries()).map(([name, v]) => ({
      name,
      friendly: friendlyName(name),
      health: healthFromSaturation(v.maxSat),
      saturation: v.maxSat,
      errors: v.errors,
    }));
  }, [scenarioResults]);

  const topRisks = useMemo(() => {
    const risks: string[] = [];
    for (const sr of scenarioResults) {
      if (sr.peakBackpressure >= 0.7) {
        risks.push(
          `The "${sr.name}" scenario pushed the system to ${(sr.peakBackpressure * 100).toFixed(0)}% capacity, risking slowdowns for end users.`
        );
      }
      if (sr.totalErrors > 0 && sr.totalWrites > 0) {
        const pct = ((sr.totalErrors / sr.totalWrites) * 100).toFixed(1);
        risks.push(
          `${pct}% of operations failed during "${sr.name}" — users would experience errors.`
        );
      }
    }
    for (const f of forecasts) {
      if (!f.estimatedStable) {
        risks.push(
          `At ${f.multiplier}x traffic, ${friendlyName(f.topBottleneck)} will become a bottleneck and the system will degrade.`
        );
      }
    }
    return risks.slice(0, 5);
  }, [scenarioResults, forecasts]);

  return (
    <div className="space-y-5 rounded-xl border-2 border-indigo-100 bg-gradient-to-b from-indigo-50/50 to-white p-6">
      <div className="flex items-center gap-3">
        <div className="flex h-8 w-8 items-center justify-center rounded-lg bg-indigo-100">
          <svg
            className="h-4 w-4 text-indigo-600"
            fill="none"
            viewBox="0 0 24 24"
            strokeWidth={2}
            stroke="currentColor"
          >
            <path
              strokeLinecap="round"
              strokeLinejoin="round"
              d="M3.75 3v11.25A2.25 2.25 0 0 0 6 16.5h2.25M3.75 3h-1.5m1.5 0h16.5m0 0h1.5m-1.5 0v11.25A2.25 2.25 0 0 1 18 16.5h-2.25m-7.5 0h7.5m-7.5 0-1 3m8.5-3 1 3m0 0 .5 1.5m-.5-1.5h-9.5m0 0-.5 1.5m.75-9 3-3 2.148 2.148A12.061 12.061 0 0 1 16.5 7.605"
            />
          </svg>
        </div>
        <div>
          <h2 className="text-lg font-semibold tracking-tight">
            Executive Summary
          </h2>
          <p className="text-xs text-muted-foreground">
            Non-technical overview for stakeholders and decision-makers
          </p>
        </div>
      </div>

      {/* Overall Health */}
      <div
        className={cn(
          "flex items-start gap-4 rounded-lg border p-4",
          healthColor(overall.health)
        )}
      >
        <div
          className={cn("mt-0.5 h-4 w-4 shrink-0 rounded-full", healthDot(overall.health))}
        />
        <div>
          <h3 className="font-semibold">
            Overall System Health:{" "}
            {overall.health === "healthy"
              ? "Good"
              : overall.health === "caution"
                ? "Needs Attention"
                : "Critical"}
          </h3>
          <p className="mt-0.5 text-sm">{overall.summary}</p>
        </div>
      </div>

      {/* Component Health Grid */}
      {componentHealth.length > 0 && (
        <div>
          <h3 className="mb-3 text-sm font-semibold text-gray-700">
            System Components at a Glance
          </h3>
          <div className="grid grid-cols-2 gap-3 sm:grid-cols-3 lg:grid-cols-6">
            {componentHealth.map((c) => (
              <div
                key={c.name}
                className={cn(
                  "flex flex-col items-center gap-1.5 rounded-lg border p-3 text-center",
                  healthColor(c.health)
                )}
              >
                <div
                  className={cn("h-3 w-3 rounded-full", healthDot(c.health))}
                />
                <span className="text-xs font-semibold">{c.friendly}</span>
                <span className="text-[10px] leading-tight opacity-75">
                  {healthLabel(c.health)}
                </span>
                <span className="text-[10px] tabular-nums opacity-60">
                  {(c.saturation * 100).toFixed(0)}% utilization
                </span>
              </div>
            ))}
          </div>
        </div>
      )}

      {/* Scaling Readiness */}
      <div>
        <h3 className="mb-3 text-sm font-semibold text-gray-700">
          Can We Handle More Traffic?
        </h3>
        <div className="space-y-2">
          {forecasts.map((f) => {
            const v = scalingVerdict(f);
            return (
              <div
                key={f.multiplier}
                className={cn(
                  "flex items-start gap-3 rounded-lg border px-4 py-3",
                  healthColor(v.health)
                )}
              >
                <div
                  className={cn(
                    "mt-0.5 h-3 w-3 shrink-0 rounded-full",
                    healthDot(v.health)
                  )}
                />
                <div className="min-w-0 flex-1">
                  <div className="flex items-center gap-2">
                    <span className="font-semibold">{f.multiplier}x current traffic</span>
                    <Badge
                      className={
                        v.health === "healthy"
                          ? "bg-green-200 text-green-800"
                          : v.health === "caution"
                            ? "bg-amber-200 text-amber-800"
                            : "bg-red-200 text-red-800"
                      }
                    >
                      {v.label}
                    </Badge>
                  </div>
                  <p className="mt-0.5 text-sm opacity-80">{v.detail}</p>
                </div>
              </div>
            );
          })}
        </div>
      </div>

      {/* Scenario Highlights */}
      <div>
        <h3 className="mb-3 text-sm font-semibold text-gray-700">
          Scenario Test Results
        </h3>
        <div className="space-y-2">
          {scenarioResults.map((sr) => {
            const v = scenarioVerdict(sr);
            return (
              <div
                key={sr.name}
                className={cn(
                  "flex items-start gap-3 rounded-lg border px-4 py-3",
                  healthColor(v.health)
                )}
              >
                <div
                  className={cn(
                    "mt-0.5 h-3 w-3 shrink-0 rounded-full",
                    healthDot(v.health)
                  )}
                />
                <div className="min-w-0 flex-1">
                  <div className="flex items-center gap-2">
                    <span className="font-medium">{sr.name}</span>
                    <Badge
                      className={
                        v.health === "healthy"
                          ? "bg-green-200 text-green-800"
                          : v.health === "caution"
                            ? "bg-amber-200 text-amber-800"
                            : "bg-red-200 text-red-800"
                      }
                    >
                      {v.label}
                    </Badge>
                  </div>
                  <p className="mt-0.5 text-sm opacity-80">{v.detail}</p>
                </div>
              </div>
            );
          })}
        </div>
      </div>

      {/* Key Risks */}
      {topRisks.length > 0 && (
        <div>
          <h3 className="mb-3 text-sm font-semibold text-gray-700">
            Key Risks
          </h3>
          <ul className="space-y-1.5">
            {topRisks.map((risk, i) => (
              <li key={i} className="flex items-start gap-2 text-sm text-gray-600">
                <span className="mt-1.5 h-1.5 w-1.5 shrink-0 rounded-full bg-red-400" />
                {risk}
              </li>
            ))}
          </ul>
        </div>
      )}

      {/* Action Items */}
      {recommendations.length > 0 && (
        <div>
          <h3 className="mb-3 text-sm font-semibold text-gray-700">
            What Should We Do?
          </h3>
          <ul className="space-y-1.5">
            {recommendations.slice(0, 6).map((r, i) => (
              <li key={i} className="flex items-start gap-2 text-sm text-gray-600">
                <span className="mt-1.5 h-1.5 w-1.5 shrink-0 rounded-full bg-indigo-400" />
                <span>
                  <span className="font-medium text-gray-800">
                    {friendlyName(r.component)}:
                  </span>{" "}
                  Change {r.setting} from {r.currentValue} to{" "}
                  <span className="font-medium text-green-700">
                    {r.recommendedValue}
                  </span>
                  {r.reason ? ` — ${r.reason}` : ""}
                </span>
              </li>
            ))}
          </ul>
        </div>
      )}

      <p className="border-t pt-3 text-[11px] text-muted-foreground">
        This summary is auto-generated from the stress simulation. Scroll down
        for detailed technical charts, tables, and the full report.
      </p>
    </div>
  );
}

// ────────────────── Scenario Selector ──────────────────

function ScenarioSelector({
  scenarios,
  selected,
  onToggle,
  disabled,
}: {
  scenarios: ScenarioInfo[];
  selected: Set<string>;
  onToggle: (name: string) => void;
  disabled: boolean;
}) {
  return (
    <div className="rounded-lg border bg-background p-4">
      <h3 className="mb-3 text-sm font-medium text-muted-foreground">
        Select Scenarios
      </h3>
      <div className="flex flex-wrap gap-2">
        {scenarios.map((s) => (
          <label
            key={s.name}
            className={cn(
              "flex cursor-pointer items-center gap-2 rounded-md border px-3 py-2 text-sm transition-colors",
              selected.has(s.name)
                ? "border-primary bg-primary/5 text-primary"
                : "border-border text-muted-foreground hover:bg-accent",
              disabled && "pointer-events-none opacity-60"
            )}
          >
            <input
              type="checkbox"
              checked={selected.has(s.name)}
              onChange={() => onToggle(s.name)}
              disabled={disabled}
              className="h-3.5 w-3.5 rounded border-gray-300 accent-primary"
            />
            <span>{s.name}</span>
            <span className="text-xs text-muted-foreground">
              ({s.durationMinutes}m)
            </span>
          </label>
        ))}
      </div>
    </div>
  );
}

function ScenarioProgress({
  scenario,
  progress,
  elapsed,
}: {
  scenario: { name: string; description: string; durationSeconds: number };
  progress: number;
  elapsed: number;
}) {
  return (
    <div className="rounded-lg border bg-background p-4">
      <div className="mb-2 flex items-center justify-between text-sm">
        <div className="flex items-center gap-2">
          <span className="relative flex h-2.5 w-2.5">
            <span className="absolute inline-flex h-full w-full animate-ping rounded-full bg-green-400 opacity-75" />
            <span className="relative inline-flex h-2.5 w-2.5 rounded-full bg-green-500" />
          </span>
          <span className="font-medium">{scenario.name}</span>
        </div>
        <span className="text-muted-foreground">
          {formatTime(elapsed)} / {formatTime(scenario.durationSeconds)}
        </span>
      </div>
      <div className="h-2 overflow-hidden rounded-full bg-muted">
        <div
          className="h-full rounded-full bg-primary transition-all duration-300"
          style={{ width: `${progress}%` }}
        />
      </div>
      <p className="mt-1 text-xs text-muted-foreground">
        {scenario.description}
      </p>
    </div>
  );
}

function ChartCard({
  title,
  children,
}: {
  title: string;
  children: React.ReactNode;
}) {
  return (
    <div className="rounded-lg border bg-background p-4">
      <h3 className="mb-3 text-sm font-medium">{title}</h3>
      <div className="h-52">{children}</div>
    </div>
  );
}

const CHART_COLORS = {
  p50: "#22c55e",
  p95: "#f59e0b",
  p99: "#ef4444",
  primary: "#6366f1",
  secondary: "#06b6d4",
  tertiary: "#8b5cf6",
  redFill: "#ef4444",
  greenFill: "#22c55e",
  yellowFill: "#f59e0b",
};

function ApiLatencyChart({ ticks }: { ticks: TickPayload[] }) {
  const data = useMemo(
    () =>
      ticks.map((t, i) => {
        const api = t.snapshots.find((s) => s.name === "API Load Generator");
        return {
          t: i,
          p50: api?.p50LatencyMs ?? 0,
          p95: api?.p95LatencyMs ?? 0,
          p99: api?.p99LatencyMs ?? 0,
        };
      }),
    [ticks]
  );

  return (
    <ResponsiveContainer width="100%" height="100%">
      <AreaChart data={data}>
        <CartesianGrid strokeDasharray="3 3" opacity={0.3} />
        <XAxis dataKey="t" tick={{ fontSize: 10 }} />
        <YAxis tick={{ fontSize: 10 }} />
        <Tooltip contentStyle={{ fontSize: 12 }} />
        <Area
          type="monotone"
          dataKey="p99"
          stroke={CHART_COLORS.p99}
          fill={CHART_COLORS.p99}
          fillOpacity={0.15}
          name="P99"
        />
        <Area
          type="monotone"
          dataKey="p95"
          stroke={CHART_COLORS.p95}
          fill={CHART_COLORS.p95}
          fillOpacity={0.2}
          name="P95"
        />
        <Area
          type="monotone"
          dataKey="p50"
          stroke={CHART_COLORS.p50}
          fill={CHART_COLORS.p50}
          fillOpacity={0.3}
          name="P50"
        />
      </AreaChart>
    </ResponsiveContainer>
  );
}

function KafkaLagChart({ ticks }: { ticks: TickPayload[] }) {
  const consumerGroups = useMemo(() => {
    const groups = new Set<string>();
    ticks.forEach((t) =>
      Object.keys(t.consumerMetrics).forEach((k) => groups.add(k))
    );
    return Array.from(groups);
  }, [ticks]);

  const data = useMemo(
    () =>
      ticks.map((t, i) => {
        const point: Record<string, number> = { t: i };
        consumerGroups.forEach((g) => {
          point[g] = t.consumerMetrics[g]?.lag ?? 0;
        });
        return point;
      }),
    [ticks, consumerGroups]
  );

  const colors = [
    CHART_COLORS.primary,
    CHART_COLORS.secondary,
    CHART_COLORS.tertiary,
  ];

  return (
    <ResponsiveContainer width="100%" height="100%">
      <LineChart data={data}>
        <CartesianGrid strokeDasharray="3 3" opacity={0.3} />
        <XAxis dataKey="t" tick={{ fontSize: 10 }} />
        <YAxis tick={{ fontSize: 10 }} />
        <Tooltip contentStyle={{ fontSize: 11 }} />
        {consumerGroups.map((g, i) => (
          <Line
            key={g}
            type="monotone"
            dataKey={g}
            stroke={colors[i % colors.length]}
            dot={false}
            strokeWidth={1.5}
            name={g.replace("inventory-", "")}
          />
        ))}
      </LineChart>
    </ResponsiveContainer>
  );
}

function CdcQueueChart({ ticks }: { ticks: TickPayload[] }) {
  const data = useMemo(
    () =>
      ticks.map((t, i) => {
        const cdc = t.snapshots.find((s) => s.name === "Debezium CDC");
        return { t: i, queue: cdc?.currentQueueDepth ?? 0 };
      }),
    [ticks]
  );

  return (
    <ResponsiveContainer width="100%" height="100%">
      <AreaChart data={data}>
        <CartesianGrid strokeDasharray="3 3" opacity={0.3} />
        <XAxis dataKey="t" tick={{ fontSize: 10 }} />
        <YAxis tick={{ fontSize: 10 }} />
        <Tooltip contentStyle={{ fontSize: 12 }} />
        <Area
          type="monotone"
          dataKey="queue"
          stroke={CHART_COLORS.primary}
          fill={CHART_COLORS.primary}
          fillOpacity={0.2}
          name="WAL Backlog"
        />
      </AreaChart>
    </ResponsiveContainer>
  );
}

function EsIndexingChart({ ticks }: { ticks: TickPayload[] }) {
  const data = useMemo(
    () =>
      ticks.map((t, i) => {
        const es = t.snapshots.find((s) => s.name === "Elasticsearch");
        return {
          t: i,
          queue: es?.currentQueueDepth ?? 0,
          indexed: es?.totalProcessed ?? 0,
        };
      }),
    [ticks]
  );

  return (
    <ResponsiveContainer width="100%" height="100%">
      <ComposedChart data={data}>
        <CartesianGrid strokeDasharray="3 3" opacity={0.3} />
        <XAxis dataKey="t" tick={{ fontSize: 10 }} />
        <YAxis yAxisId="left" tick={{ fontSize: 10 }} />
        <YAxis yAxisId="right" orientation="right" tick={{ fontSize: 10 }} />
        <Tooltip contentStyle={{ fontSize: 12 }} />
        <Bar
          yAxisId="left"
          dataKey="queue"
          fill={CHART_COLORS.secondary}
          fillOpacity={0.5}
          name="Queue Depth"
        />
        <Line
          yAxisId="right"
          type="monotone"
          dataKey="indexed"
          stroke={CHART_COLORS.p50}
          dot={false}
          strokeWidth={1.5}
          name="Total Indexed"
        />
      </ComposedChart>
    </ResponsiveContainer>
  );
}

function RedisMemoryChart({ ticks }: { ticks: TickPayload[] }) {
  const data = useMemo(
    () =>
      ticks.map((t, i) => {
        const redis = t.snapshots.find((s) => s.name === "Redis");
        return {
          t: i,
          memoryMb:
            redis && redis.saturationLevel > 0
              ? Math.round(redis.saturationLevel * 512)
              : 0,
        };
      }),
    [ticks]
  );

  return (
    <ResponsiveContainer width="100%" height="100%">
      <AreaChart data={data}>
        <CartesianGrid strokeDasharray="3 3" opacity={0.3} />
        <XAxis dataKey="t" tick={{ fontSize: 10 }} />
        <YAxis tick={{ fontSize: 10 }} />
        <Tooltip contentStyle={{ fontSize: 12 }} />
        <ReferenceLine
          y={512}
          stroke={CHART_COLORS.p99}
          strokeDasharray="5 5"
          label={{ value: "Max 512 MB", fontSize: 10, fill: CHART_COLORS.p99 }}
        />
        <Area
          type="monotone"
          dataKey="memoryMb"
          stroke={CHART_COLORS.tertiary}
          fill={CHART_COLORS.tertiary}
          fillOpacity={0.2}
          name="Memory (MB)"
        />
      </AreaChart>
    </ResponsiveContainer>
  );
}

function BackpressureChart({ ticks }: { ticks: TickPayload[] }) {
  const data = useMemo(
    () =>
      ticks.map((t, i) => ({
        t: i,
        bp: Math.round(t.backpressureLevel * 100),
      })),
    [ticks]
  );

  return (
    <ResponsiveContainer width="100%" height="100%">
      <AreaChart data={data}>
        <defs>
          <linearGradient id="bpGradient" x1="0" y1="1" x2="0" y2="0">
            <stop offset="0%" stopColor={CHART_COLORS.greenFill} />
            <stop offset="50%" stopColor={CHART_COLORS.yellowFill} />
            <stop offset="100%" stopColor={CHART_COLORS.redFill} />
          </linearGradient>
        </defs>
        <CartesianGrid strokeDasharray="3 3" opacity={0.3} />
        <XAxis dataKey="t" tick={{ fontSize: 10 }} />
        <YAxis domain={[0, 100]} tick={{ fontSize: 10 }} unit="%" />
        <Tooltip contentStyle={{ fontSize: 12 }} />
        <ReferenceLine
          y={70}
          stroke={CHART_COLORS.p99}
          strokeDasharray="3 3"
        />
        <ReferenceLine
          y={30}
          stroke={CHART_COLORS.p95}
          strokeDasharray="3 3"
        />
        <Area
          type="monotone"
          dataKey="bp"
          stroke={CHART_COLORS.p99}
          fill="url(#bpGradient)"
          fillOpacity={0.3}
          name="Backpressure %"
        />
      </AreaChart>
    </ResponsiveContainer>
  );
}

function ComponentStatusTable({ tick }: { tick: TickPayload }) {
  return (
    <div className="rounded-lg border bg-background">
      <div className="border-b px-4 py-3">
        <h3 className="text-sm font-medium">Component Status</h3>
      </div>
      <div className="overflow-x-auto">
        <table className="w-full text-sm">
          <thead>
            <tr className="border-b bg-muted/50 text-left text-xs font-medium text-muted-foreground">
              <th className="px-4 py-2">Component</th>
              <th className="px-4 py-2 text-right">P50 (ms)</th>
              <th className="px-4 py-2 text-right">P95 (ms)</th>
              <th className="px-4 py-2 text-right">P99 (ms)</th>
              <th className="px-4 py-2 text-right">Saturation</th>
              <th className="px-4 py-2 text-right">Queue</th>
              <th className="px-4 py-2 text-right">Processed</th>
              <th className="px-4 py-2 text-right">Errors</th>
            </tr>
          </thead>
          <tbody>
            {tick.snapshots.map((s) => (
              <tr key={s.name} className="border-b last:border-b-0">
                <td className="px-4 py-2 font-medium">{s.name}</td>
                <td className="px-4 py-2 text-right tabular-nums">
                  {s.p50LatencyMs.toFixed(1)}
                </td>
                <td className="px-4 py-2 text-right tabular-nums">
                  {s.p95LatencyMs.toFixed(1)}
                </td>
                <td className="px-4 py-2 text-right tabular-nums">
                  {s.p99LatencyMs.toFixed(1)}
                </td>
                <td className="px-4 py-2 text-right">
                  <SaturationBadge level={s.saturationLevel} />
                </td>
                <td className="px-4 py-2 text-right tabular-nums">
                  {s.currentQueueDepth.toLocaleString()}
                </td>
                <td className="px-4 py-2 text-right tabular-nums">
                  {s.totalProcessed.toLocaleString()}
                </td>
                <td className="px-4 py-2 text-right tabular-nums">
                  <span className={s.totalErrors > 0 ? "text-red-600" : ""}>
                    {s.totalErrors.toLocaleString()}
                  </span>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
}

function ScenarioResultsTable({
  results,
}: {
  results: {
    name: string;
    totalWrites: number;
    totalErrors: number;
    peakBackpressure: number;
    bottlenecks: {
      component: string;
      score: number;
    }[];
    alertCount: number;
  }[];
}) {
  return (
    <div className="rounded-lg border bg-background">
      <div className="border-b px-4 py-3">
        <h3 className="text-sm font-medium">Scenario Results</h3>
      </div>
      <div className="overflow-x-auto">
        <table className="w-full text-sm">
          <thead>
            <tr className="border-b bg-muted/50 text-left text-xs font-medium text-muted-foreground">
              <th className="px-4 py-2">Scenario</th>
              <th className="px-4 py-2 text-right">Writes</th>
              <th className="px-4 py-2 text-right">Errors</th>
              <th className="px-4 py-2 text-right">Peak BP</th>
              <th className="px-4 py-2">Top Bottleneck</th>
              <th className="px-4 py-2 text-right">Alerts</th>
            </tr>
          </thead>
          <tbody>
            {results.map((r) => (
              <tr key={r.name} className="border-b last:border-b-0">
                <td className="px-4 py-2 font-medium">{r.name}</td>
                <td className="px-4 py-2 text-right tabular-nums">
                  {r.totalWrites.toLocaleString()}
                </td>
                <td className="px-4 py-2 text-right tabular-nums">
                  <span className={r.totalErrors > 0 ? "text-red-600" : ""}>
                    {r.totalErrors.toLocaleString()}
                  </span>
                </td>
                <td className="px-4 py-2 text-right">
                  <SaturationBadge level={r.peakBackpressure} />
                </td>
                <td className="px-4 py-2">
                  {r.bottlenecks[0]?.component ?? "—"}
                </td>
                <td className="px-4 py-2 text-right tabular-nums">
                  <span className={r.alertCount > 0 ? "text-red-600" : ""}>
                    {r.alertCount}
                  </span>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
}

function ForecastTable({
  forecasts,
}: {
  forecasts: {
    multiplier: number;
    totalWritesPerSec: number;
    totalReadsPerSec: number;
    estimatedStable: boolean;
    topBottleneck: string;
    topBottleneckScore: number;
    backpressureLevel: number;
  }[];
}) {
  return (
    <div className="rounded-lg border bg-background">
      <div className="border-b px-4 py-3">
        <h3 className="text-sm font-medium">Scaling Forecast</h3>
      </div>
      <div className="overflow-x-auto">
        <table className="w-full text-sm">
          <thead>
            <tr className="border-b bg-muted/50 text-left text-xs font-medium text-muted-foreground">
              <th className="px-4 py-2">Scale</th>
              <th className="px-4 py-2">Status</th>
              <th className="px-4 py-2 text-right">Writes/sec</th>
              <th className="px-4 py-2 text-right">Reads/sec</th>
              <th className="px-4 py-2">Top Bottleneck</th>
              <th className="px-4 py-2 text-right">Score</th>
              <th className="px-4 py-2 text-right">Backpressure</th>
            </tr>
          </thead>
          <tbody>
            {forecasts.map((f) => (
              <tr key={f.multiplier} className="border-b last:border-b-0">
                <td className="px-4 py-2 font-medium">{f.multiplier}x</td>
                <td className="px-4 py-2">
                  <Badge
                    className={
                      f.estimatedStable
                        ? "bg-green-100 text-green-700"
                        : "bg-red-100 text-red-700"
                    }
                  >
                    {f.estimatedStable ? "STABLE" : "UNSTABLE"}
                  </Badge>
                </td>
                <td className="px-4 py-2 text-right tabular-nums">
                  {f.totalWritesPerSec.toLocaleString()}
                </td>
                <td className="px-4 py-2 text-right tabular-nums">
                  {f.totalReadsPerSec.toLocaleString()}
                </td>
                <td className="px-4 py-2">{f.topBottleneck}</td>
                <td className="px-4 py-2 text-right tabular-nums">
                  {f.topBottleneckScore.toFixed(3)}
                </td>
                <td className="px-4 py-2 text-right">
                  <SaturationBadge level={f.backpressureLevel} />
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
}

function RecommendationsTable({
  recommendations,
}: {
  recommendations: {
    component: string;
    setting: string;
    currentValue: string;
    recommendedValue: string;
    reason: string;
  }[];
}) {
  return (
    <div className="rounded-lg border bg-background">
      <div className="border-b px-4 py-3">
        <h3 className="text-sm font-medium">Infrastructure Recommendations</h3>
      </div>
      <div className="overflow-x-auto">
        <table className="w-full text-sm">
          <thead>
            <tr className="border-b bg-muted/50 text-left text-xs font-medium text-muted-foreground">
              <th className="px-4 py-2">Component</th>
              <th className="px-4 py-2">Setting</th>
              <th className="px-4 py-2">Current</th>
              <th className="px-4 py-2">Recommended</th>
              <th className="px-4 py-2">Reason</th>
            </tr>
          </thead>
          <tbody>
            {recommendations.map((r, i) => (
              <tr key={i} className="border-b last:border-b-0">
                <td className="px-4 py-2 font-medium">{r.component}</td>
                <td className="px-4 py-2">{r.setting}</td>
                <td className="px-4 py-2 text-muted-foreground tabular-nums">
                  {r.currentValue}
                </td>
                <td className="px-4 py-2 font-medium text-green-600 tabular-nums">
                  {r.recommendedValue}
                </td>
                <td className="px-4 py-2 text-muted-foreground">{r.reason}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
}

function ReportSection({ markdown }: { markdown: string }) {
  return (
    <div className="rounded-lg border bg-background">
      <div className="flex items-center justify-between border-b px-4 py-3">
        <h3 className="text-sm font-medium">Final Report</h3>
        <Button
          variant="outline"
          size="sm"
          onClick={() => {
            const blob = new Blob([markdown], { type: "text/markdown" });
            const url = URL.createObjectURL(blob);
            const a = document.createElement("a");
            a.href = url;
            a.download = "stress-report.md";
            a.click();
            URL.revokeObjectURL(url);
          }}
        >
          Download
        </Button>
      </div>
      <pre className="max-h-96 overflow-auto p-4 text-xs leading-relaxed text-muted-foreground">
        {markdown}
      </pre>
    </div>
  );
}

function SaturationBadge({ level }: { level: number }) {
  const pct = (level * 100).toFixed(0);
  const color =
    level < 0.3
      ? "bg-green-100 text-green-700"
      : level < 0.7
        ? "bg-yellow-100 text-yellow-700"
        : "bg-red-100 text-red-700";
  return <Badge className={color}>{pct}%</Badge>;
}

function formatTime(seconds: number) {
  const m = Math.floor(seconds / 60);
  const s = Math.floor(seconds % 60);
  return `${m}:${s.toString().padStart(2, "0")}`;
}

const RETAILER_PRESETS = [100, 500, 1_000, 5_000, 10_000, 50_000, 100_000];
const INVENTORY_PRESETS = [10_000, 100_000, 500_000, 1_000_000, 5_000_000, 10_000_000, 50_000_000];
const MENU_PRESETS = [10_000, 100_000, 500_000, 1_000_000, 5_000_000, 10_000_000];
const USER_PRESETS = [1_000, 5_000, 10_000, 50_000, 100_000, 500_000];

function formatNumber(n: number): string {
  if (n >= 1_000_000) return `${n / 1_000_000}M`;
  if (n >= 1_000) return `${n / 1_000}K`;
  return n.toString();
}

function PresetSlider({
  label,
  presets,
  index,
  onChange,
  disabled,
}: {
  label: string;
  presets: number[];
  index: number;
  onChange: (i: number) => void;
  disabled: boolean;
}) {
  return (
    <div className="space-y-2">
      <div className="flex items-center justify-between">
        <span className="text-sm font-medium">{label}</span>
        <span className="rounded-full bg-accent px-2.5 py-0.5 text-xs font-semibold tabular-nums">
          {formatNumber(presets[index])}
        </span>
      </div>
      <input
        type="range"
        min={0}
        max={presets.length - 1}
        step={1}
        value={index}
        onChange={(e) => onChange(Number(e.target.value))}
        disabled={disabled}
        className="w-full accent-primary disabled:opacity-50"
      />
      <div className="flex justify-between text-[10px] text-muted-foreground">
        <span>{formatNumber(presets[0])}</span>
        <span>{formatNumber(presets[presets.length - 1])}</span>
      </div>
    </div>
  );
}
