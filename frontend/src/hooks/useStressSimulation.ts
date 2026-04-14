import { useCallback, useRef, useState } from "react";
import { startSimulation, type RunParams } from "@/api/stress";

export interface SnapshotPayload {
  name: string;
  p50LatencyMs: number;
  p95LatencyMs: number;
  p99LatencyMs: number;
  saturationLevel: number;
  currentQueueDepth: number;
  totalProcessed: number;
  totalErrors: number;
}

export interface ConsumerLagPayload {
  lag: number;
  consumed: number;
  latencyMs: number;
}

export interface TickPayload {
  scenario: string;
  elapsedSeconds: number;
  durationSeconds: number;
  writeCount: number;
  readCount: number;
  writeErrors: number;
  avgWriteLatencyMs: number;
  avgReadLatencyMs: number;
  backpressureLevel: number;
  outboxEventsGenerated: number;
  debeziumProcessed: number;
  snapshots: SnapshotPayload[];
  consumerMetrics: Record<string, ConsumerLagPayload>;
}

export interface ScenarioStartPayload {
  name: string;
  description: string;
  durationSeconds: number;
  writesPerSec: number;
  readsPerSec: number;
}

export interface ScenarioEndPayload {
  name: string;
  totalWrites: number;
  totalErrors: number;
  peakBackpressure: number;
  bottlenecks: BottleneckPayload[];
  alertCount: number;
}

export interface BottleneckPayload {
  component: string;
  score: number;
  maxSaturation: number;
  maxP99LatencyMs: number;
  totalErrors: number;
}

export interface ForecastPayload {
  multiplier: number;
  totalWritesPerSec: number;
  totalReadsPerSec: number;
  estimatedStable: boolean;
  topBottleneck: string;
  topBottleneckScore: number;
  backpressureLevel: number;
}

export interface RecommendationPayload {
  component: string;
  setting: string;
  currentValue: string;
  recommendedValue: string;
  reason: string;
}

const MAX_TICKS = 300;

export function useStressSimulation() {
  const [isRunning, setIsRunning] = useState(false);
  const [currentScenario, setCurrentScenario] =
    useState<ScenarioStartPayload | null>(null);
  const [ticks, setTicks] = useState<TickPayload[]>([]);
  const [scenarioResults, setScenarioResults] = useState<ScenarioEndPayload[]>(
    []
  );
  const [forecasts, setForecasts] = useState<ForecastPayload[]>([]);
  const [recommendations, setRecommendations] = useState<
    RecommendationPayload[]
  >([]);
  const [reportAvailable, setReportAvailable] = useState(false);
  const [runId, setRunId] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);
  const esRef = useRef<EventSource | null>(null);

  const stop = useCallback(() => {
    esRef.current?.close();
    esRef.current = null;
    setIsRunning(false);
  }, []);

  const start = useCallback(
    async (params: RunParams) => {
      stop();
      setTicks([]);
      setScenarioResults([]);
      setForecasts([]);
      setRecommendations([]);
      setReportAvailable(false);
      setError(null);
      setCurrentScenario(null);

      try {
        const { runId: id } = await startSimulation(params);
        setRunId(id);
        setIsRunning(true);

        const es = new EventSource(`/api/stress/stream/${id}`);
        esRef.current = es;

        es.addEventListener("tick", (e) => {
          const tick: TickPayload = JSON.parse(e.data);
          setTicks((prev) => {
            const next = [...prev, tick];
            return next.length > MAX_TICKS ? next.slice(-MAX_TICKS) : next;
          });
        });

        es.addEventListener("scenario-start", (e) => {
          const payload: ScenarioStartPayload = JSON.parse(e.data);
          setCurrentScenario(payload);
          setTicks([]);
        });

        es.addEventListener("scenario-end", (e) => {
          const payload: ScenarioEndPayload = JSON.parse(e.data);
          setScenarioResults((prev) => [...prev, payload]);
        });

        es.addEventListener("forecast", (e) => {
          const payload: ForecastPayload[] = JSON.parse(e.data);
          setForecasts(payload);
        });

        es.addEventListener("recommendations", (e) => {
          const payload: RecommendationPayload[] = JSON.parse(e.data);
          setRecommendations(payload);
        });

        es.addEventListener("complete", () => {
          setReportAvailable(true);
          setIsRunning(false);
          setCurrentScenario(null);
          es.close();
          esRef.current = null;
        });

        es.addEventListener("error", (e) => {
          if (es.readyState === EventSource.CLOSED) {
            setIsRunning(false);
            esRef.current = null;
          }
        });
      } catch (err: unknown) {
        const msg = err instanceof Error ? err.message : "Unknown error";
        setError(msg);
        setIsRunning(false);
      }
    },
    [stop]
  );

  return {
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
  };
}
