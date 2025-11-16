import Link from "next/link";
import { ArrowLeft } from "lucide-react";
import { getProduct } from "@/lib/api/products";
import { updateProductAction } from "@/actions/products";
import { ProductForm } from "@/components/products/ProductForm";

export default async function EditProductPage({
  params,
}: {
  params: { id: string };
}) {
  const product = await getProduct(params.id);

  const handleUpdate = async (formData: FormData) => {
    "use server";
    await updateProductAction(params.id, formData);
  };

  return (
    <div className="container mx-auto max-w-3xl px-4 py-8">
      <div className="mb-6">
        <Link
          href={`/products/${params.id}`}
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
