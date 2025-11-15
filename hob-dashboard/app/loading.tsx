export default function Loading() {
  return (
    <div className="space-y-8">
      <div>
        <div className="h-8 w-48 bg-gray-200 rounded animate-pulse" />
        <div className="h-4 w-96 bg-gray-200 rounded animate-pulse mt-2" />
      </div>

      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
        {[...Array(4)].map((_, i) => (
          <div key={i} className="h-32 bg-white border border-gray-200 rounded-lg animate-pulse" />
        ))}
      </div>

      <div className="grid gap-4 md:grid-cols-3">
        {[...Array(3)].map((_, i) => (
          <div key={i} className="h-32 bg-white border border-gray-200 rounded-lg animate-pulse" />
        ))}
      </div>

      <div className="h-96 bg-white border border-gray-200 rounded-lg animate-pulse" />
    </div>
  );
}
