import { useState } from "react";
import { Link } from "react-router-dom";
import { Plus, Search } from "lucide-react";
import { useSearchMenus } from "@/hooks/useMenu";
import MenuCard from "@/components/menu/MenuCard";
import RetailerSelector from "@/components/RetailerSelector";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Spinner } from "@/components/ui/spinner";

export default function MenuListPage() {
  const [query, setQuery] = useState("");
  const [ownerId, setOwnerId] = useState("");
  const [page, setPage] = useState(1);
  const pageSize = 20;

  const { data, isLoading, isError } = useSearchMenus({
    q: query || undefined,
    ownerId: ownerId || undefined,
    page,
    size: pageSize,
  });

  const totalPages = data ? Math.ceil(data.total / pageSize) : 0;

  return (
    <div className="mx-auto max-w-6xl space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold">Menus</h1>
          <p className="text-sm text-muted-foreground">
            {data ? `${data.total} menu${data.total !== 1 ? "s" : ""}` : ""}
          </p>
        </div>
        <Link to="/menus/new">
          <Button>
            <Plus className="h-4 w-4" />
            Create Menu
          </Button>
        </Link>
      </div>

      <div className="flex flex-col gap-3 sm:flex-row sm:items-center">
        <div className="relative flex-1">
          <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
          <Input
            placeholder="Search menus..."
            value={query}
            onChange={(e) => {
              setQuery(e.target.value);
              setPage(1);
            }}
            className="pl-9"
          />
        </div>
        <RetailerSelector
          value={ownerId}
          onChange={(o) => {
            setOwnerId(o);
            setPage(1);
          }}
          className="w-48"
        />
      </div>

      {isLoading && (
        <div className="flex justify-center py-16">
          <Spinner />
        </div>
      )}

      {isError && (
        <p className="py-12 text-center text-sm text-muted-foreground">
          Failed to load menus. Make sure the API and Elasticsearch are running.
        </p>
      )}

      {data && (
        <>
          <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
            {data.items.map((menu) => (
              <MenuCard key={menu.id} menu={menu} />
            ))}
          </div>

          {data.items.length === 0 && (
            <p className="py-12 text-center text-sm text-muted-foreground">
              No menus found. Try adjusting your filters or create a new menu.
            </p>
          )}

          {totalPages > 1 && (
            <div className="flex items-center justify-center gap-2 pt-4">
              <Button
                variant="outline"
                size="sm"
                disabled={page <= 1}
                onClick={() => setPage(page - 1)}
              >
                Previous
              </Button>
              <span className="text-sm text-muted-foreground">
                Page {page} of {totalPages}
              </span>
              <Button
                variant="outline"
                size="sm"
                disabled={page >= totalPages}
                onClick={() => setPage(page + 1)}
              >
                Next
              </Button>
            </div>
          )}
        </>
      )}
    </div>
  );
}
