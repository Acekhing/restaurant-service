import client from "./client";
import type {
  Timetable,
  CreateTimetableRequest,
  UpdateTimetableRequest,
} from "@/types/timetable";

export async function listTimetables(params: {
  ownerId: string;
}): Promise<Timetable[]> {
  const { data } = await client.get<Timetable[]>("/timetables", { params });
  return data;
}

export async function getTimetable(id: string): Promise<Timetable> {
  const { data } = await client.get<Timetable>(`/timetables/${id}`);
  return data;
}

export async function createTimetable(
  body: CreateTimetableRequest
): Promise<{ id: string }> {
  const { data } = await client.post<{ id: string }>("/timetables", body);
  return data;
}

export async function updateTimetable(
  id: string,
  body: UpdateTimetableRequest
): Promise<void> {
  await client.put(`/timetables/${id}`, body);
}

export async function deleteTimetable(id: string): Promise<void> {
  await client.delete(`/timetables/${id}`);
}
