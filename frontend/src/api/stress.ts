import axios from "axios";

const stressClient = axios.create({
  baseURL: "/api/stress",
  headers: { "Content-Type": "application/json" },
});

export interface ScenarioInfo {
  name: string;
  description: string;
  durationMinutes: number;
  writeMultiplier: number;
  readMultiplier: number;
  failureComponent: string | null;
}

export interface RunResponse {
  runId: string;
}

export async function getScenarios(): Promise<ScenarioInfo[]> {
  const { data } = await stressClient.get<ScenarioInfo[]>("/scenarios");
  return data;
}

export interface RunParams {
  scenarioNames: string[];
  retailers?: number;
  inventoryItems?: number;
  menus?: number;
  concurrentUsers?: number;
}

export async function startSimulation(
  params: RunParams
): Promise<RunResponse> {
  const { data } = await stressClient.post<RunResponse>("/run", params);
  return data;
}

export async function getReport(runId: string): Promise<string> {
  const { data } = await stressClient.get<string>(`/report/${runId}`);
  return data;
}
