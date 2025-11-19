import Link from "next/link";
import { ArrowLeft } from "lucide-react";
import { getProduct } from "@/lib/api/products";
import { updateProductAction } from "@/actions/products";
import { ProductForm } from "@/components/products/ProductForm";

interface PageProps {
  params: Promise<{ id: string }>;
}

export default async function EditProductPage({ params }: PageProps) {
  const { id } = await params;
  const product = await getProduct(id);

  const handleUpdate = async (formData: FormData) => {
    "use server";
    await updateProductAction(id, formData);
  };

  return (
    <div className="container mx-auto max-w-3xl px-4 py-8">
      <div className="mb-6">
        <Link
          href={`/products/${id}`}
          className="inline-flex items-center text-sm text-blue-600 hover:text-blue-800"
        >
          <ArrowLeft className="mr-1 h-4 w-4" />
          Back to Product
        </Link>
      </div>

      <h1 className="mb-6 text-3xl font-bold">Edit Product</h1>

      <ProductForm product={product} action={handleUpdate} />
    </div>
  );
}
